<template>
    <div>

        <b-tabs id="view" ref="view" v-model="currentViewIndex">

            <b-tab title="JSON">

                <b-container fluid class="m-3 pr-3">


                    <div v-show="!formRepresentation">
                        <b-button size="sm"
                                  @click="copyToClipboard"
                                  v-b-tooltip.hover.html.bottom
                                  title="Copy the data to the clipboard">
                            Copy
                        </b-button>
                        <b-button
                                size="sm"
                                @click="saveToFile"
                                v-b-tooltip.hover.html.bottom
                                title="Save the data to a file">
                            Save
                        </b-button>

                        <b-button size="sm" @click="prettyprint = !prettyprint" v-if="!prettyprint">Raw</b-button>
                        <b-button size="sm" variant="outline" @click="prettyprint = !prettyprint" v-else>Pretty Print
                        </b-button>
                        <br/>


                        <pre v-if="prettyprint">{{ representation }}</pre>
                        <pre v-html="htmlRepresentation" v-else/>
                    </div>

                    <Form :representation="representation"
                          :formRepresentation="formRepresentation"
                          :on-cancel="onClose"
                          v-if="formRepresentation"/>

                </b-container>

            </b-tab>

            <b-tab title="Headers">

                <headers title="Request Headers" :headers="requestHeaders" class="m-3"/>
                <hr/>
                <headers title="Response Headers" :headers="responseHeaders" class="m-3"/>

            </b-tab>

            <b-tab title="Logout">
                <b-container fluid>
                    <Logout class="mt-3" :on-logout="onLogout"/>
                </b-container>
            </b-tab>
        </b-tabs>

    </div>
</template>

<script>

    import axios from 'axios';
    import {linkifyToSelf} from '../filters/linkifyWithClientRouting';
    import {findLinkRel} from "../filters/makeButtonOnLinkifyLinkRels";
    import Logout from './Logout.vue';
    import Headers from './Headers.vue';
    import Form from './Form.vue';
    import {copyToClipboard, saveToFile} from "../lib/raw-helpers";
    import {LinkedRepresentation, CollectionRepresentation} from 'semantic-link';
    import * as link from 'semantic-link';
    import FormAction from './FormAction.vue';
    import Vue from 'vue';
    import {fromUriList, makeUriList} from "../lib/dragAndDropModel";
    import {compare, deepClone} from 'fast-json-patch'
    import bTabs from 'bootstrap-vue/es/components/tabs/tabs';
    import bTab from 'bootstrap-vue/es/components/tabs/tab';
    import bTooltip from 'bootstrap-vue/es/components/tooltip/tooltip';
    import {log} from 'logger';
    import EventBus, {authConfirmed} from '../lib/EventBus';
    import {loader} from 'semantic-link-cache';

    import FormDragDrop from './FormDragDrop.vue';

    export default {
        props: {
            apiUri: {type: String},
        },
        components: {Logout, Headers, Form, FormDragDrop, bTabs, bTab, bTooltip},
        data() {
            return {
                /**
                 * In memory representation based on the apiUri
                 * @type {LinkedRepresentation}
                 */
                representation: null,
                /**
                 * Linkify HTML template string constructed from {@link representation}. Vue then mounts this.
                 * @type {string}
                 */
                htmlRepresentation: null,
                /**
                 * @type {AxiosResponse.headers}
                 */
                responseHeaders: null,
                /**
                 * @type {AxiosRequestConfig.headers}
                 */
                requestHeaders: null,
                /**
                 * @type {CollectionRepresentation}
                 */
                formRepresentation: null,
                /**
                 * Default Accept header that asks for HTML > JSON > XML > anything
                 * @type {string}
                 */
                defaultAccept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95',
                /**
                 * Print print UI flag. True = formatted down the page, False = no extra white spaces
                 * @type {boolean}
                 */
                prettyprint: false,
                /**
                 * Tracks and sets the view (tabs). It is matched as the fragment in the url against the title of tab
                 * @type {number}
                 */
                currentViewIndex: 0,
            }
        },
        created() {
            this.getRepresentation();
        },
        mounted: function () {

            /////////////////////////////
            //
            // Authentication
            // ==============
            //
            // After (re)authentication load up the representation (we could have just redirected)

            EventBus.$on(authConfirmed, this.getRepresentation);

            /////////////////////////////
            //
            // Client-side routing
            // ===================
            //
            // Simple client router mixed in with tabs for navigation through the url/window.location
            //

            /**
             * Incoming setting of the client-side state view.
             *
             * We are matching the url fragment against a tab index
             *
             * ** Warning ** Tabs must be setup correctly for this to work in tabs and tab
             *
             *    `<b-tabs id="view" ref="view" v-model="currentViewIndex">`
             *
             *    id: used to pick out the correct tabs layout
             *    ref: used when lost click context
             *    currentViewIndex: bound to the tabs for setting tabs
             *
             *    `<b-tab title="Json">`
             *
             *    title: bound to the url fragment in lowercase (eg url#json)
             */
            const viewTitle = window.location.hash.replace('#', '');
            this.currentViewIndex = this.$refs.view.tabs.findIndex(tab => tab.title.toLowerCase() === viewTitle);
            log.debug(`[View] set ${viewTitle}`);

            /**
             *
             * The user agent fires a popstate event when the user navigates through their history,
             * whether backwards or forwards, provided it isn’t taking the user away from the
             * current page. That is, all those pushStates we called will keep the user on the current page,
             * so the 'popstate' event will fire for each history item they pop through.
             *
             * Pages can add state objects between their entry in the session history and the next (“forward”)
             * entry. These are then returned to the script when the user (or script) goes back in the history,
             * thus enabling authors to use the “navigation” metaphor even in one-page applications.
             *
             * ** WARNING **
             * If you look at the developer console in Chrome when you load the page for the first time, you’ll
             * see the popstate event fired immediately, before you’ve even clicked a link. This is because
             * Chrome considers the initial page load to be a change in state, and so it fires the event.
             * In this instance, the state property is null, but thankfully the updateContent function deals
             * with this. Keep this in mind when developing as it could catch you out, especially if other
             * browsers assume this behavior.
             *
             * However, this listener is not being registered early enough (ie on mounted) and thus
             * does not receive this event.
             *
             * @see http://html5doctor.com/history-api/
             *
             */
            window.addEventListener('popstate', (e) => {
                if (e.state) {
                    this.currentViewIndex = e.state.tabIndex || 0;
                }
            });

            /**
             * Listening for the global `changed::tab` even that provides us with context and the tab selection.
             *
             * In doing so, it does mean that we need to ensure we are looking at the correct tabs which is identified
             * by its `id` which needs to be set in the template (otherwise, if the app has more tabs it will collide).
             *
             */
            this.$root.$on('changed::tab', (tabs, tabIndex, tab) => {

                /**
                 * There is one problem: using the back button creates a 'double'/bounce click effect. The tabs component watches
                 * for any tab changes and then emits events for tab:click and $root:changed::tab. We need to work out
                 * whether the origin of the event was a Back/Forward button or a user click. This is done via checking the
                 * document uri (which has the previous orig) and match against the current. If they are the same then
                 * we are going to say that it was Back/Forward event so we need to debounce it.
                 *
                 * @param {string} title
                 * @return {boolean}
                 */
                const isTabChange = title => {
                    return title !== document.URL.replace(document.referrer, '').replace('#', '')
                };

                if (tabs.id === 'view') {

                    const title = tab.title.toLowerCase();
                    log.debug(`[View] changed '${title}'`);

                    if (isTabChange(title)) {
                        log.debug(`[History] pushstate '${title}'`);
                        window.history.pushState({tabIndex}, title, `${window.location.pathname}#${title}`);
                    }

                } else {
                    log.debug(`[View] tabs not processed ${tabs.id}`);
                }

            })
        },
        computed: {
            isLoaded() {
                return this.representation != null;
            },
        },
        methods: {
            /**
             *
             * Takes two representations and merges into a distinct set of uri-list that needs to be PUT
             * onto the collection
             *
             * @param {string} data uri-list (not an array)
             * @param {CollectionRepresentation} resource
             * @return {string} uri-list (not an array) (no comments)
             */
            toUriList(data, resource) {
                // make uri-list from a collection
                const itemUris = [resource.items].map(({id}) => id);
                // make a distinct list of uris and return as uri-list
                return makeUriList([...new Set([...itemUris, ...fromUriList(data)])]);
            },
            /**
             *
             * Takes to representations and merges into a JSON Patch Document to PATCH against the collection
             *
             * Note: this does not currently allow for removing items from a collection
             *
             * @param {string} data uri-list (not an array)
             * @param {CollectionRepresentation} resource
             * @return {*} Json Patch Document
             */
            toPatchDocument(data, resource) {
                // create a copy as the template
                const newCollection = deepClone(resource);

                // Add any new uris that need to be added to the collection
                fromUriList(data).forEach(uri => newCollection.items.push({id: uri}));

                // generate the Json Patch document
                return compare(resource, newCollection);
            },
            /**
             * On a button click of an action, GET the form so that it can be rendered
             * @param {string} rel link relation
             * @param {?string} type media type
             */
            getForm(rel, type) {
                loader.schedule(() => link.get(this.representation, rel, type))
                    .then(/** @type {AxiosResponse} */response => {
                        this.formRepresentation = response.data;
                    });
            },
            /**
             * Try and delete a representation. Because all 'self' links have a delete option, we'll try and then if
             * successful move the user back to the 'up' link relation of the deleted resource or if it failed (ie this
             * isn't an available option) show the message to the user.
             *
             * TODO: note this could try OPTIONS first
             *
             * @param {string} rel
             * @return {Promise}
             */
            tryDelete(rel) {

                return loader.schedule(() => link.delete(this.representation, rel))
                    .then(/** @type {AxiosResponse|Error} */response => {

                        // appropriate repsonses from a deleted resource
                        // see https://stackoverflow.com/questions/2342579/http-status-code-for-update-and-delete
                        if (response.status === 204 || response.status === 200 || response.status === 202) {

                            // 202 is that is accepted to be processed (TODO: retry mechanism that isn't immediate)
                            if (response.status === 202) {
                                this.$notify({type: 'info', text: 'Resource marked for deletion. Confirming deletion'})
                            }

                            // check that it has in fact been deleted
                            return loader.schedule(() => link.get(this.representation, /^self$/))
                            // it is an error if it succeeds
                                .then(() => this.$notify({
                                    type: 'error',
                                    text: 'This item was unable to be deleted and still exists'
                                }))
                                .catch(/** @type {AxiosResponse} */response => {
                                    // success if it isn't found or no content
                                    if (response.status === 404 || response.status === 204) {

                                        const uri = link.getUri(this.representation, /up/) || '/';

                                        this.$notify({
                                            type: 'success',
                                            title: 'Item successfully deleted.',
                                            text: `Redirecting to <a href="${uri}">item</a>`

                                        });

                                        setTimeout(() => {
                                            window.location.href = uri;
                                        }, 3000);


                                    } else {
                                        log.warn('Request is in weird state');
                                    }
                                });
                        } else {
                            this.$notify({
                                type: 'error',
                                title: response.statusText || response.message || '',
                                text: 'This should be fixed'
                            });
                        }

                    })
                    .catch(/** @type {AxiosResponse|*} */response => {
                        this.$notify({
                            title: response.statusText,
                            text: 'You can\'t delete this.',
                            type: 'error'
                        });
                    });
            },
            /**
             * GET the resource and layout the JSON as html with update, created and remove
             * return {Promise}
             */
            getRepresentation() {

                log.warn('Fetching representation ');

                return loader.schedule(() => axios.get(this.apiUri, {reponseHeaders: {'Accept': this.defaultAccept}}))
                    .then(/** @type {AxiosResponse} */response => {
                        this.responseHeaders = response.headers;
                        this.representation = response.data;
                        this.htmlRepresentation = linkifyToSelf(response.data);
                        this.requestHeaders = response.config.headers;

                        this.$nextTick(() => {

                            /**
                             * Now that we have the resource loaded, wait for the render (ie nextTick) and attach
                             * all the actions we want onto the links relations.
                             *
                             * Below is a list of all the different actions available:
                             *   - buttons (ie forms that are filled out by the user
                             *   - drop (and drag) targets
                             *
                             * These will get placed in-line as components. You should be able to register
                             * more as you go. Hopefully, this pluggable approach is readable and workable.
                             *
                             * Add components dynamically is a well known approach
                             * @see see https://css-tricks.com/creating-vue-js-component-instances-programmatically/
                             */

                            const ButtonComponent = Vue.extend(FormAction);
                            const DropTargetComponent = Vue.extend(FormDragDrop);

                            [
                                ['edit-form', undefined, new ButtonComponent({
                                    propsData: {
                                        type: 'Edit',
                                        onClick: this.getForm,
                                        rel: 'edit-form',
                                        title: 'Click to update existing information'
                                    }
                                })],
                                ['search', undefined, new ButtonComponent({
                                    propsData: {
                                        type: 'Search',
                                        onClick: this.getForm,
                                        rel: 'search',
                                        title: 'Click to open a search form'
                                    }
                                })],
                                ['create-form', undefined, new ButtonComponent({
                                    propsData: {
                                        type: 'Add',
                                        onClick: this.getForm,
                                        rel: 'create-form',
                                        title: 'Click to enter new information to create a new item'
                                    }
                                })],
                                ['self', undefined, new ButtonComponent({
                                    propsData: {
                                        type: 'Delete',
                                        onClick: this.tryDelete,
                                        rel: 'self',
                                        title: 'Click to delete'
                                    }
                                })],
                                ['edit-form', 'text/uri-list', new DropTargetComponent({
                                    propsData: {
                                        method: 'put',
                                        representation: this.representation,
                                        map: this.toUriList,
                                        mediaType: 'text/uri-list',
                                        accept: 'text/uri-list',
                                        title: 'Drag a uri to create (via PUT uri-list)'
                                    }
                                })],
                                ['edit-form', 'application/json-patch+json', new DropTargetComponent({
                                    propsData: {
                                        method: 'patch',
                                        representation: this.representation,
                                        map: this.toPatchDocument,
                                        mediaType: 'application/json-patch+json',
                                        accept: 'text/uri-list',
                                        title: 'Drag a uri to create (via PATCH)'
                                    }
                                })],
                            ].forEach(([rel, mediaType, component]) => findLinkRel(rel, mediaType)
                                .forEach(el => el && el.insertBefore(component.$mount().$el, el.firstChild)));


                        });

                        this.resetForm();
                    })
                    .catch(/** @type {AxiosError} */error => {

                        this.responseHeaders = error.response.headers;
                        this.requestHeaders = error.response.config.headers;

                        this.htmlRepresentation = `<div>${error.response.statusText}</div>`;

                        this.$notify({
                            title: 'Error',
                            text: error.response.statusText,
                            type: 'error'
                        })
                    });
            },
            onUpdated() {
                return this.getRepresentation();
            },
            onClose() {
                this.resetForm();
            },
            resetForm() {
                this.formRepresentation = null;
            },
            copyToClipboard() {
                copyToClipboard(JSON.stringify(this.representation, null, 2));
                this.$notify('Copied to clipboard');

            },
            saveToFile() {
                saveToFile(
                    JSON.stringify(this.representation, null, 2),
                    (this.representation.name || this.representation.title || 'unknown') + '.json',
                    'application/json');
            },
            onLogout() {
                this.$notify({
                    title: 'Successfully logged out',
                    text: 'Redirect back to resource ...',
                    type: 'success'
                });

                setTimeout(() => {
                    this.currentViewIndex = 0;
                }, 1500)
            }
        }
    }
</script>

<style scoped>
    pre {
        padding: 5px;
        margin: 5px;
    }

    .string {
        color: green;
    }

    .number {
        color: darkorange;
    }

    .boolean {
        color: blue;
    }

    .null {
        color: magenta;
    }

    .key {
        color: red;
    }

</style>