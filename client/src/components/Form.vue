<template>

    <b-form @submit="submit">

        <b-form-group v-for="item in formRepresentation.items"
                      :key="item.name"
                      :label="item.name">
            <!-- DATE TIME -->
            <!-- date time pickers are unreliable across browsers and devices -->
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

            <!-- RADIO -->
            <b-form-radio-group
                    v-else-if="mapApiToUiType(item.type) === 'check'"
                    v-model="formObj[item.name]"
                    buttons
                    button-variant="outline-primary"
                    size="sm"
                    :options="[{text: 'True', value: true},{text: 'False', value: false}]"
            ></b-form-radio-group>

            <!-- SELECT -->
            <b-form-select
                    v-else-if="mapApiToUiType(item.type) === 'select'"
                    v-model="formObj[item.name]"
            >
                <option :value="null" disabled>-- Please select an option --</option>
                <template v-for="option in item.items">
                    <option :value="option.value">{{ option.label}}</option>
                </template>
            </b-form-select>

            <!-- TEXT -->
            <b-form-input
                    v-else
                    :id="`input-1-${item.name}`"
                    :type="mapApiToUiType(item.type)"
                    v-model="formObj[item.name]"
                    :required="item.required"
                    :placeholder="item.description"
            ></b-form-input>
            <span class="highlight"></span><span class="bar"></span>


        </b-form-group>
        <b-button variant="outline" @click="onCancel" v-if="!noCancel">
            <Back h="26px" w="26px"/>
        </b-button>
        <b-button variant="primary" @click="submit">{{ submitTitle }}</b-button>
    </b-form>

</template>

<script>
    import {mapApiToUiType} from '../lib/form-type-mappings';
    import {Datetime} from 'vue-datetime';
    import {DateTime as LuxonDateTime} from 'luxon'
    // You need a specific loader for CSS files
    import 'vue-datetime/dist/vue-datetime.css'
    import FormService from "../lib/FormService";
    import {filter, getUri} from 'semantic-link';
    import {log} from 'logger';

    import Back from 'vue-ionicons/dist/md-arrow-round-back.vue';
    import Ok from 'vue-ionicons/dist/md-send.vue';


    export default {
        name: "Form",
        components: {datetime: Datetime, Back, Ok},
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
             * @type {FormRepresentation}
             */
            formRepresentation: {
                type: Object,
                required: true
            },
            /**
             * Flag to display the cancel button
             * Default is to show the cancel button
             * @type {boolean}
             */
            noCancel: {
                type: Boolean,
                required: false,
                default: false
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
             * Callback before the form is submitted
             * @event FormResource.onSubmit
             */
            onSubmit: {
                type: Function,
                required: false,
                default: () => {
                }
            },
            /**
             * An optional callback once the form has been successfully submitted. By default the action is to
             * - show a notify message
             * - redirect the page to the collection (POST) or item (PUT)
             * @event FormResource.onSuccess
             */
            onSuccess: {
                type: Function,
                required: false,
                default: null
            },
            /**
             * An optional callback once the form has been submitted but fails. By default the action is to
             * - show a notify message
             * @event FormResource.onFailure
             */
            onFailure: {
                type: Function,
                required: false,
                default: null
            },
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
            this.formObj = FormService.makeFormObj(this.formRepresentation, this.representation);
        },
        computed: {
            /**
             * Defaults to 'Submit' and is overriden by the link relation 'submit' name value
             * @example { rel: 'submit', name: 'Search', href: ...
             * @type {string}
             */
            submitTitle() {
                // KLUDGE: only support one/first submit link rel d
                const [link] = filter(this.formRepresentation, /^submit$/);
                return (link || {}).title || 'Submit';
            }
        },
        methods: {
            /**
             * On all the fields are entered then either make a PUT (edit/update) or POST (create, search) based on
             * the referring representation
             */
            submit() {

                this.onSubmit();

                log.debug(`[Form] submit ${getUri(this.representation, 'self')}`);

                return FormService.submitForm(this.formObj, this.formRepresentation, this.representation)
                    .then(/** @type {AxiosResponse} */response => {

                        this.$notify({
                            title: `${response.statusText} <a href="${response.headers.location}">item<a>`,
                            text: 'Redirecting ...',
                            type: 'success'
                        });

                        log.debug(`[Form] return [${response.status}] `);

                        // on success if updated return to item (204) otherwise go to new (201)
                        // scope it here otherwise SemanticLink looks to loose scope (??) in the setTimeout
                        const returnUri = response.status === 201
                            ? response.headers.location
                            : getUri(this.representation, 'self');

                        setTimeout(() => {
                            window.location = returnUri;
                        }, 2000)

                    })
                    .catch(/** @type {AxiosResponse|string} */err => {
                        if (this.onFailure) {
                            this.onFailure(err)
                        } else {
                            const message = err.statusText || err.message || '';
                            this.$notify({title: 'Error submitting', text: message, type: 'error'});
                        }
                    });

            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>