<template>
    <div>

        <b-tabs>

            <b-tab title="JSON" active>

                <b-container fluid class="m-3 pr-3">

                    <Form :representation="representation"
                          :formRepresentation="formRepresentation"
                          :on-updated="onUpdated"
                          :on-cancel="onClose"
                          :formRel="formRel"
                          v-if="formRepresentation"/>

                    <div v-show="!formRepresentation">
                        <b-button size="sm" @click="copyToClipboard">Copy</b-button>
                        <b-button size="sm" @click="saveToFile">Save</b-button>
                        <br/>
                        <pre v-html="htmlRepresentation"/>
                    </div>

                </b-container>

            </b-tab>

            <b-tab title="Raw">

                <b-container fluid class="m-3 pr-3">

                    <b-button size="sm" @click="copyToClipboard">Copy</b-button>
                    <b-button size="sm" @click="saveToFile">Save</b-button>

                    <b-button size="sm" @click="prettyprint = !prettyprint" v-if="!prettyprint">Pretty Print</b-button>
                    <b-button size="sm" variant="outline" @click="prettyprint = !prettyprint" v-else>Pretty Print</b-button>
                    <br/>
                    <div class="mt-3">
                        <pre v-if="prettyprint">{{ representation }}</pre>
                        <code v-else>{{ representation }}</code>
                    </div>

                </b-container>
            </b-tab>

            <b-tab title="Headers">

                <headers title="Request Headers" :headers="requestHeaders" class="m-3"/>
                <hr/>
                <headers title="Response Headers" :headers="responseHeaders" class="m-3"/>

            </b-tab>

            <b-tab title="Logout">
                <b-container fluid>
                    <Logout class="mt-3"/>
                </b-container>
            </b-tab>
        </b-tabs>

    </div>
</template>

<script>

    import axios from 'axios';
    import { linkifyToSelf } from '../filters/linkifyWithClientRouting';
    import { makeButtonOnLinkifyLinkRels } from "../filters/makeButtonOnLinkifyLinkRels";
    import { link, SemanticLink } from 'semanticLink';
    import Logout from './Logout.vue';
    import Headers from './Headers.vue';
    import Form from './Form.vue';
    import { copyToClipboard, saveToFile } from "../lib/raw-helpers";


    export default {
        props: {
            apiUri: {type: String},
        },
        components: {Logout, Headers, Form},
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
                 * @type {string}
                 */
                formRel: null,
                /**
                 * Default Accept header that asks for HTML > JSON > XML > anything
                 * @type {string}
                 */
                defaultAccept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95',
                /**
                 * Print print UI flag. True = formatted down the page, False = no extra white spaces
                 * @type {boolean}
                 */
                prettyprint: false
            };
        },
        created() {
            return this.getRepresentation();
        },
        methods: {
            /**
             * On a button click of an action, GET the form so that it can be rendered
             * @param {string} rel link relation
             */
            getForm(rel) {
                const vm = this;
                link.get(this.representation, rel)
                    .then(response => {
                        vm.formRepresentation = response.data;
                        vm.formRel = rel;
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

                return link.delete(this.representation, /^self$/)
                    .then(/** @type {AxiosResponse|Error} */response => {

                        // appropriate repsonses from a deleted resource
                        // seehttps://stackoverflow.com/questions/2342579/http-status-code-for-update-and-delete
                        if (response.status === 204 || response.status === 200 || response.status === 202) {

                            // 202 is that is accepted to be processed (TODO: retry mechanism that isn't immediate)
                            if (response.status === 202) {
                                this.$notify({type: 'info', text: 'Resource marked for deletion. Confirming deletion'})
                            }

                            // check that it has in fact been deleted
                            return link.get(this.representation, /^self$/)
                                // it is an error if it succeeds
                                .then(() => this.$notify({
                                    type: 'error',
                                    text: 'This item was unable to be deleted and still exists'
                                }))
                                .catch(/** @type {AxiosResponse} */response => {
                                    // success if it isn't found or no content
                                    if (response.status === 404 || response.status === 204) {

                                        const uri = SemanticLink.getUri(this.representation, /up/) || '/';

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
                            text: 'You can\'t delete this, sorry',
                            type: 'error'
                        });
                    });
            },
            /**
             * GET the resource and layout the JSON as html with update, created and remove
             * return {Promise}
             */
            getRepresentation() {
                return axios.get(this.apiUri, {reponseHeaders: {'Accept': this.defaultAccept}})
                    .then(/** @type {AxiosResponse} */response => {
                        this.responseHeaders = response.headers;
                        this.representation = (response.data);
                        this.htmlRepresentation = linkifyToSelf(response.data);
                        this.requestHeaders = response.config.headers;

                        this.$nextTick(() => {
                            makeButtonOnLinkifyLinkRels('edit-form', {onClick: this.getForm, title: 'Edit'});
                            makeButtonOnLinkifyLinkRels('create-form', {onClick: this.getForm, title: 'Add'});
                            makeButtonOnLinkifyLinkRels('search', {onClick: this.getForm, title: 'Search'});
                            makeButtonOnLinkifyLinkRels('self', {onClick: this.tryDelete, title: 'Delete'});
                        });

                        this.resetForm();
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