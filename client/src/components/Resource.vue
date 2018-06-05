<template>
    <div>

        <b-tabs>

            <b-tab title="JSON" active>

                <b-container fluid>

                    <Form :representation="representation"
                          :formRepresentation="formRepresentation"
                          :on-updated="onUpdated"
                          :on-cancel="onClose"
                          :formRel="formRel"
                          v-if="formRepresentation"/>

                    <pre v-html="htmlRepresentation" ref="representation"/>

                </b-container>

            </b-tab>

            <b-tab title="Raw">
                {{ representation }}
            </b-tab>

            <b-tab title="Headers">
                <headers title="Request Headers" :headers="requestHeaders"/>
                <headers title="Response Headers" :headers="reponseHeaders"/>
            </b-tab>

            <b-tab title="Logout">
                <Logout></Logout>
            </b-tab>
        </b-tabs>

    </div>
</template>

<script>

    import axios from 'axios';
    import { linkifyToSelf } from '../filters/linkifyWithClientRouting';
    import { makeButtonOnLinkifyLinkRels } from "../filters/makeButtonOnLinkifyLinkRels";
    import { link } from 'semanticLink';
    import Logout from './Logout.vue';
    import Headers from './Headers.vue';
    import Form from './Form.vue';


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
                reponseHeaders: null,
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
                defaultAccept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95'
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
            tryDeleteRepresentation(rel) {
                return link.delete(this.representation, /self/)
                    .then(/** @type {AxiosResponse} */response => {

                        if (response.status === 204 || response.status === 200 || response.status === 202) {

                            if (response.status === 202) {
                                this.$notify({type: 'info', text: 'Resource marked for deletion. Confirming deletion'})
                            }

                            return link.get(this.representation, /self/)
                                .then(() => this.$notify({
                                    type: 'error',
                                    text: 'This item was unable to be deleted and still exists'
                                }))
                                .catch(/** @type {AxiosResponse} */response => {
                                    if (response.status === 404 || response.status === 204) {
                                        this.$notify({
                                            type: 'success',
                                            text: 'Item successfully deleted. Redirecting to \'up\' link relations'
                                        });
                                    } else {
                                        log.warn('Request is in weird state');
                                    }
                                });
                        } else {
                            this.$notify({
                                type: 'warning',
                                title: `Response code: ${response.status}`,
                                text: 'This is weird and should be understood'
                            });
                        }


                        // try again just to ensure it is deleted
                        // if not stay showing and show an error that it didn't delete
                    })
                    .catch(/** @type {AxiosResponse} */response => {
                        this.$notify({title: response.statusText, text: 'You can\'t delete this, sorry', type: 'info'});
                    });
            },
            /**
             * GET the resource and layout the JSON as html with update, created and remove
             * return {Promise}
             */
            getRepresentation() {
                return axios.get(this.apiUri, {reponseHeaders: {'Accept': this.defaultAccept}})
                    .then(/** @type {AxiosResponse} */response => {
                        this.reponseHeaders = response.reponseHeaders;
                        this.representation = (response.data);
                        this.htmlRepresentation = linkifyToSelf(response.data);
                        this.requestHeaders = response.config.reponseHeaders;

                        this.$nextTick(() => {
                            makeButtonOnLinkifyLinkRels('edit-form', {onClick: this.getForm, title: 'Edit'});
                            makeButtonOnLinkifyLinkRels('create-form', {onClick: this.getForm, title: 'Add'});
                            makeButtonOnLinkifyLinkRels('self', {
                                onClick: this.tryDeleteRepresentation,
                                title: 'Delete'
                            });
                        });

                        this.resetForm();
                    });
            },
            onUpdated() {
                this.getRepresentation();
            },
            onClose() {
                this.resetForm();
            },
            resetForm() {
                this.formRepresentation = null;
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