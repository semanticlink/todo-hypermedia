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
                            makeButtonOnLinkifyLinkRels('edit-form', {onClick: this.getForm});
                            makeButtonOnLinkifyLinkRels('create-form', {onClick: this.getForm, title: 'Add'});
                            // makeButtonOnLinkifyLinkRels('self', {onClick: this.getForm, title: 'Delete'});
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