<template>
    <div>

        <b-tabs>

            <b-tab title="JSON" active>

                <b-container fluid>

                    <Form :representation="representation"
                          :formRepresentation="formRepresentation"
                          :on-updated="onUpdated"
                          :formRel="formRel"
                          v-if="formRepresentation"/>

                    <pre v-html="htmlRepresentation" ref="representation"/>

                </b-container>

            </b-tab>

            <b-tab title="Raw">
                {{ representation }}
            </b-tab>

            <b-tab title="Headers">
                <headers title="Request Headers" :headers="requestheaders"/>
                <headers title="Response Headers" :headers="headers"/>
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
    import { link } from 'semanticLink';
    import Logout from './Logout.vue';
    import Headers from './Headers.vue';
    import Form from './Form.vue';
    import Vue from 'vue';

    /**
     *
     * @param rel
     * @returns {HTMLElement[]}
     */
    const findLinkRel = (rel) => {
        return [...document.querySelectorAll('span.string')]      // get all the divs in an array
            .filter(div => div.innerText.includes(rel))               // get their contents
            .map(div => div.nextElementSibling.nextElementSibling);
    };

    const addButtonToHref = (el, propsData = {}) => {

        // https://stackoverflow.com/questions/35927664/how-to-add-dynamic-components-partials-in-vue-js
        // https://css-tricks.com/creating-vue-js-component-instances-programmatically/
        const Btn = Vue.extend({
            template: '<b-button size="sm" variant="secondary" @click="onClick(rel)">{{title}}</b-button>',
            props: {
                title: {default: 'Edit'},
                onClick: {
                    type: Function, default: () => {
                    }
                },
                rel: {default: 'edit-form'}
            }
        });

        const instance = new Btn({propsData});

        el.insertBefore(instance.$mount().$el, el.firstChild);
    };

    const makeButtonOnLinkRels = (rel, propsData) => findLinkRel(rel).forEach(el => addButtonToHref(el, Object.assign({}, {rel}, propsData)));

    export default {
        props: {
            apiUri: {type: String},
        },
        components: {Logout, Headers, Form},
        data() {
            return {
                response: {},
                htmlRepresentation: null,
                headers: null,
                requestheaders: null,
                representation: null,
                formRepresentation: null,
                formRel: null,
                accept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95'
            };
        },
        created() {
            return this.onUpdated()
        },
        methods: {
            getForm(rel) {
                const vm = this;
                link.get(this.representation, rel)
                    .then(response => {
                        vm.formRepresentation = response.data;
                        vm.formRel = rel;
                    });
            },
            onUpdated(representation, response) {
                axios.get(this.apiUri, {headers: {'Accept': this.accept}})
                    .then(response => {
                        this.response = response;
                        this.headers = response.headers;
                        this.representation = (response.data);
                        this.htmlRepresentation = linkifyToSelf(response.data);
                        this.requestheaders = response.config.headers;

                        this.$nextTick(() => {
                            makeButtonOnLinkRels('edit-form', {onClick: this.getForm});
                            makeButtonOnLinkRels('create-form', {onClick: this.getForm, title: 'Add'});
                        });

                        this.resetForm();
                    });
            },
            update() {
                return;
            },
            resetForm(){
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