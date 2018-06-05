<template>
    <div>

        <div>
            <b-list-group>
                <b-list-group-item>{{ self }}</b-list-group-item>
            </b-list-group>
            <b-btn v-if="canEdit" @click="makeEdit">Edit</b-btn>
            <b-btn v-if="canCreate">Create</b-btn>
        </div>

        <div>
            <div v-for="(value, key) in requestheaders">
                <b-badge variant="primary" pill>{{key}}</b-badge>
                {{value}}
            </div>
        </div>

        <pre v-html="htmlRepresentation"/>

        <div>
            <div v-for="(value, key) in headers">
                <b-badge variant="primary" pill>{{key}}</b-badge>
                {{value}}
            </div>
        </div>

        <b-container fluid v-if="canEdit && editForm">
            <b-row class="my-1" v-for="item in editForm.items" :key="item.name">
                <b-col sm="3"><label :for="`type-${item.type}`">Type {{ item.name }}:</label></b-col>
                <b-col sm="9">
                    <b-form-input :id="`type-${item.type}`"
                                  :type="type(item.type)"
                                  v-model="representation[item.name]"
                                  :placeholder="`${item.description}`"/>
                </b-col>
            </b-row>
            <b-btn @click="update">Update</b-btn>
        </b-container>
        `
    </div>
</template>

<script>

    import axios from 'axios';
    import linkifyWithClientRouting from '../filters/linkifyWithClientRouting';
    import { SemanticLink, link } from 'semanticLink';

    /**
     * Maps the representation types to the known types that can be rendered (input not select at this stage)
     * @see https://bootstrap-vue.js.org/docs/components/form-input
     * TODO: move to util
     *
     * TODO:
     *   types: [ 'text', 'password', 'email', 'number', 'url', 'tel', 'date', `time`, 'range', 'color' ]
     *
     * TODO: implement mapping based on agent
     *
     *      Caveats with input types:
     *      - Not all browsers support all input types, nor do some types render in the same format across browser types/version.
     *      - Browsers that do not support a particular type will fall back to a text input type. As an example, Firefox desktop doesn't support date, datetime, or time, while Firefox mobile does.
     *      - Chrome lost support for datetime in version 26, Opera in version 15, and Safari in iOS 7. Instead of using datetime, since support should be deprecated, use date and time as two separate input types.
     *      - For date and time style input, where supported, the displayed value in the GUI may be different than what is returned by its value.
     *      - Regardless of input type, the value is always returned as a string representation.
     *
     * @param {string} t     * @returns {string}
     */
    const type = t => {
        switch (t) {
            case 'http://types/text':
                return 'text';
                break;
            case 'http://types/date':
                return 'date';
                break;

            default:
                return '';
        }

    };

    export default {
        props: {
            apiUri: {type: String},
        },
        data() {
            return {
                response: {},
                htmlRepresentation: null,
                headers: null,
                requestHeaders: null,
                representation: null,
                editForm: null,
                defaultAccept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95'
            };
        },
        computed: {
            self() {

                axios.get(this.apiUri, {headers: {'Accept': this.defaultAccept}})
                    .then(response => {
                        this.response = response;
                        this.headers = response.headers;
                        this.representation = (response.data);
                        this.htmlRepresentation = linkifyWithClientRouting(response.data);
                        this.requestheaders = response.config.headers;

                    });
                return this.apiUri;
            },
            canEdit() {
                return SemanticLink.matches(this.representation, 'edit-form');
            },
            canCreate() {
                return SemanticLink.matches(this.representation, 'create-form');
            }
        },
        methods: {
            makeEdit() {
                const vm = this;
                link.get(this.representation, 'edit-form')
                    .then(response => {
                        vm.editForm = response.data;
                    });
            },
            type(t) {
                return type(t);
            },
            update() {
                return;
            }
        }
    }
</script>

<style>
    pre {
        /*outline: 1px solid #ccc;*/
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