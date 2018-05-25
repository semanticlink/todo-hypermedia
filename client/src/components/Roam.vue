<template>
    <div>
        <div>
            {{ self }}
        </div>

        {{ requestheaders }}

        <pre v-html="representation"/>

        {{ headers}}

    </div>
</template>

<script>

    import axios from 'axios';
    import linkify from '../filters/linkify'

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