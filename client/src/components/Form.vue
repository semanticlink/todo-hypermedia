<template>

    <b-form @submit="submit">

        <b-form-group v-for="item in formRepresentation.items"
                      :key="item.name"
                      :label="item.name">
            <b-form-input :id="`input-1-${item.name}`"
                          :type="mapApiToUiType(item.type)"
                          v-model="formObj[item.name]"
                          :required="item.required"
                          :placeholder="item.description"></b-form-input>
        </b-form-group>
        <b-button type="submit" variant="primary">Submit</b-button>
    </b-form>

</template>

<script>
    import { mapApiToUiType } from '../lib/form-type-mappings';
    import { link, log } from "semanticLink";

    export default {
        name: "Form",
        props: {
            formRepresentation: {
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
            },
            formRel: {
                type: String,
                required: true
            }
        },
        data() {
            return {
                error: null,
                formObj: null
            }
        },
        created() {
            if (this.isCreateForm()) {
                this.formObj = {};
            } else if (this.isEditForm()) {
                this.formObj = Object.assign({}, this.representation);
            } else {
                log.warn('Trying to display form of unknown type');
            }
        },
        methods: {
            isCreateForm() {
                return /^create-form$/.test(this.formRel);
            },
            isEditForm() {
                return /^edit-form$/.test(this.formRel);
            },
            submit() {
                const changes = this.representation;

                let verb;
                if (this.isCreateForm()) {
                    verb = 'post';
                } else if (this.isEditForm()) {
                    verb = 'put'
                } else {
                    log.warn('Trying to display form of unkown type');
                    return
                }

                link[verb](this.representation, 'self', 'application/json', this.formObj)
                    .then(/** @type {AxiosResponse} */(r) => {
                        this.onUpdated(changes, r);
                    })
                    .catch(/** @type {AxiosError} */error => {
                        this.error = error.response.statusText;
                    });
            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>