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
                <div>{{ localDateTime.zoneName}} ({{ localDateTime.offsetNameLong}})</div>
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

    /**
     * A form has the job to POST to a collection or PUT to an item (this is by convention).
     *
     * The semantics of the form are that:
     *
     * In terms of where and how to send forms:
     *  - if link rel 'submit' use the href uri to POST to (show title 'Submit')
     *  - if no link rel 'submit' then PUT to parent resource (aka {@link this.representation} (usually an item)
     *  - if the link rel 'submit' has name attribute use that for display and override 'Submit'
     *
     * In terms of form values:
     *  - In the case of POST, start with a new object and fill out values (based on the form)
     *  - In the case of PUT, clone a new object based on the  existing item (ie prepopulate) and update values (based on the form)
     */
    export default {
        name: "Form",
        components: {datetime: Datetime},
        props: {
            /**
             * Collection or item representation.
             * @type {CollectionRepresentation|LinkedRepresentation}
             */
            representation: {
                type: Object,
                required: true
            },
            /**
             * Form that specifies the item/inputs to submit back to the server
             * @type {CollectionRepresentation}
             */
            formRepresentation: {
                type: Object,
                required: true
            },
            /**
             * @event FormResource.onUpdated
             */
            onUpdated: {
                type: Function,
                required: false,
                default: () => {
                }
            },
            /**
             * @event FormResource.onCancel
             */
            onCancel: {
                type: Function,
                required: false,
                default: () => {
                }
            },
            /**
             * @obsolete
             */
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

            /**
             * When we show a form on the screen, decide whether to clone or create an in-memory representation
             */
            if (this.isCreateForm() || this.isSearchForm()) {
                this.formObj = {};
            } else if (this.isEditForm()) {
                this.formObj = Object.assign({}, this.representation);
            } else {
                log.warn(`Trying to display form of unknown type: '${this.formRel}'`);
            }
        },
        methods: {
            isCreateForm() {
                return /^create-form$/.test(this.formRel);
            },
            isEditForm() {
                return /^edit-form$/.test(this.formRel);
            },
            isSearchForm() {
                return /^search$/.test(this.formRel);
            },
            /**
             * On all the fields are entered then either make a PUT (edit/update) or POST (create, search) based on
             * the referring representation
             */
            submit() {
                let changes = this.representation;

                let verb, message, rel, links;
                if (this.isCreateForm()) {
                    verb = 'post';
                    rel = 'submit';
                    message = 'Item created successfully';
                    links = this.formRepresentation;
                } else if (this.isSearchForm()) {
                    verb = 'post';
                    rel = 'submit';
                    message = 'Successful search';
                    links = this.formRepresentation;
                } else if (this.isEditForm()) {
                    verb = 'put';
                    rel = 'self';
                    message = 'Item updated successfully';
                    links = this.representation;
                } else {
                    log.warn(`Trying to submit form of unknown type: '${this.formRel}'`);
                    return
                }

                return link[verb](links, rel, 'application/json', this.formObj)
                    .then(/** @type {AxiosResponse} */(r) => {
                        this.$notify({text: message, type: 'success'});
                        return this.onUpdated(changes, r);
                    })
                    .catch(/** @type {AxiosResponse} */response => {
                        this.$notify({text: response.statusText, type: 'error'});

                    });
            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>