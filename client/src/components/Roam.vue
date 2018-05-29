<template>
    <div>

        <div>
            <b-list-group>
                <b-list-group-item>{{ self }}</b-list-group-item>
            </b-list-group>

        </div>

        <div>
            <div v-for="(value, key) in requestheaders">
                <b-badge variant="primary" pill>{{key}}</b-badge>
                {{value}}
            </div>
        </div>

        <pre v-html="representation"/>

        <div>
            <div v-for="(value, key) in headers">
                <b-badge variant="primary" pill>{{key}}</b-badge>
                {{value}}
            </div>
        </div>

    </div>
</template>

<script>

    import axios from 'axios';
    import linkify from '../filters/linkify';

    export default {
        props: {
            apiUri: {type: String},
        },
        data() {
            return {
                response: {},
                representation: null,
                headers: null,
                requestheaders: null
            };
        },
        computed: {
            self: function () {

                axios.get(this.apiUri)
                    .then(response => {
                        this.response = response;
                        this.headers = response.headers;
                        this.representation = linkify(response.data);
                        this.requestheaders = response.config.headers;

                    });
                return this.apiUri;
            }
        }
    };
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