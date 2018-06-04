<template>

    <b-form @submit="submit">

        <b-form-group v-for="item in form.items"
                      :key="item.name"
                      :label="item.name">
            <b-form-input :id="`input-1-${item.name}`"
                          :type="mapApiToUiType(item.type)"
                          v-model="representation[item.name]"
                          :required="item.required"
                          :placeholder="item.description"></b-form-input>
        </b-form-group>
        <b-button type="submit" variant="primary">Submit</b-button>
    </b-form>

</template>

<script>
    import { mapApiToUiType } from '../lib/form-type-mappings';
    import { link } from "semanticLink";

    export default {
        name: "Form",
        props: {
            form: {
                type: Object,
                required: true
            },
            representation: {
                type: Object,
                required: true
            },
            onUpdated: {
                type: Function,
                required: false,
                default: () => {
                }

            }
        },
        data() {
            return{
                error: null
            }
        },
        methods: {
            submit() {
                const changes = this.representation;
                link.put(this.representation, 'self', 'application/json', this.representation)
                    .then(/** @type {AxiosResponse} */(r) => {
                        this.onUpdated(changes, r);
                    })
                    .catch(/** @type {AxiosError} */error => {
                        this.error = error.response.statusText;
                    })
            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>