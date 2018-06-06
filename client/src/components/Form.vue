<template>

    <b-form @submit="submit">

        <b-form-group v-for="item in formRepresentation.items"
                      :key="item.name"
                      :label="item.name">
            <!-- date time pickers are unrealiable across browsers and devices -->
            <template v-if="mapApiToUiType(item.type) === 'date' || mapApiToUiType(item.type) === 'datetime'">
                <datetime
                        :type="mapApiToUiType(item.type)"
                        v-model="formObj[item.name]"
                        input-class="form-control"
                        :phrases="{ok: 'Continue', cancel: 'Exit'}"
                        use12-hour
                        auto
                        :min-datetime="minDatetime"
                ></datetime>
                <div>{{ localDateTime.zoneName}} ({{ localDateTime.offsetNameLong}}) </div>
            </template>
            <b-form-radio-group
                    v-else-if="mapApiToUiType(item.type) === 'check'"
                    v-model="formObj[item.name]"
                    buttons
                    button-variant="outline-primary"
                    size="sm"
                    :options="[{text: 'True', value: true},{text: 'False', value: false}]"
            ></b-form-radio-group>

            <b-form-input
                    v-else
                    :id="`input-1-${item.name}`"
                    :type="mapApiToUiType(item.type)"
                    v-model="formObj[item.name]"
                    :required="item.required"
                    :placeholder="item.description"
            ></b-form-input>

        </b-form-group>
        <b-button type="submit" variant="primary">Submit</b-button>
        <b-button variant="secondary" @click="onCancel">Cancel</b-button>
    </b-form>

</template>

<script>
    import { mapApiToUiType } from '../lib/form-type-mappings';
    import { link, log } from "semanticLink";
    import { Datetime } from 'vue-datetime';
    import { DateTime as LuxonDateTime } from 'luxon'
    // You need a specific loader for CSS files
    import 'vue-datetime/dist/vue-datetime.css'

    export default {
        name: "Form",
        components: {datetime: Datetime},
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
                formObj: {},
                minDatetime: LuxonDateTime.local().toISO(),
                localDateTime: LuxonDateTime.local()
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