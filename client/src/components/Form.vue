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
        <b-button variant="secondary" @click="onCancel">Cancel</b-button>
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
             onCancel: {
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
                /**
                 * An in-memory object of the data that we are going to send back to the server. In the case of a
                 * create form it will be empty (new) and the edit form it will be a clone of the representation
                 * to be updated.
                 *
                 * @type {*|LinkedRepresentation}
                 */
                formObj: {}
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
            /**
             * On all the fields are entered then eithe make a PUT (edit/update) or POST (create) based on
             * the referring representation
             */
            submit() {
                const changes = this.representation;

                let verb, message;
                if (this.isCreateForm()) {
                    verb = 'post';
                    message = 'Item created successfully';
                } else if (this.isEditForm()) {
                    verb = 'put'
                    message = 'Item updated successfully'
                } else {
                    log.warn('Trying to display form of unkown type');
                    return
                }

                link[verb](this.representation, 'self', 'application/json', this.formObj)
                    .then(/** @type {AxiosResponse} */(r) => {
                        this.onUpdated(changes, r);
                        this.$notify({text: message, type: 'success'});

                    })
                    .catch(/** @type {AxiosError} */error => {
                        this.$notify(error.response.statusText)
                    });
            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>