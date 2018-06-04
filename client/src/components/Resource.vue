<template>
    <div>

        <b-tabs>

            <b-tab title="JSON" active>

                <div>
                    <b-btn v-if="canEdit" @click="makeEdit">Edit</b-btn>
                    <b-btn v-if="canCreate">Create</b-btn>
                </div>

                <pre v-html="htmlRepresentation"/>

                <div>
                    <b-btn v-if="canEdit" @click="makeEdit">Edit</b-btn>
                    <b-btn v-if="canCreate">Create</b-btn>
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
    import { link, log, SemanticLink } from 'semanticLink';
    import { mapApiToUiType} from "../lib/form-type-mappings";
    import Logout from './Logout.vue';
    import Headers from './Headers.vue';

    export default {
        props: {
            apiUri: {type: String},
        },
        components: {Logout, Headers},
        data() {
            return {
                response: {},
                htmlRepresentation: null,
                headers: null,
                requestheaders: null,
                representation: null,
                editForm: null,
                accept: 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8, application/json;q=0.95'
            };
        },
        created() {
            log.info(`Resource uri: '${this.apiUri}'`);
            axios.get(this.apiUri, {headers: {'Accept': this.accept}})
                .then(response => {
                    this.response = response;
                    this.headers = response.headers;
                    this.representation = (response.data);
                    this.htmlRepresentation = linkifyToSelf(response.data);
                    this.requestheaders = response.config.headers;

                });

        },
        computed: {
            canEdit() {
                return SemanticLink.matches(this.representation, 'edit-form');
            },
            canCreate() {
                return SemanticLink.matches(this.representation, 'create-form');
            }
        }
        ,
        methods: {
            makeEdit() {
                const vm = this;
                link.get(this.representation, 'edit-form')
                    .then(response => {
                        vm.editForm = response.data;
                    });
            },
            type:  mapApiToUiType,
            update() {
                return;
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