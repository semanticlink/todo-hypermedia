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
        <b-button type="submit" variant="primary">{{ submitTitle }}</b-button>
        <b-button variant="secondary" @click="onCancel" v-if="!noCancel">Cancel</b-button>
    </b-form>

</template>

<script>
    import { mapApiToUiType } from '../lib/form-type-mappings';
    import { link, SemanticLink } from "semanticLink";
    import { Datetime } from 'vue-datetime';
    import { DateTime as LuxonDateTime } from 'luxon'
    // You need a specific loader for CSS files
    import 'vue-datetime/dist/vue-datetime.css'

    /**
     * A form has the job to POST to a collection or PUT to an item (this is by convention).
     *
     * The semantics of the form are that:
     *
     * In terms of the display label:
     * ==============================
     *
     *  1. Default value is 'Submit'
     *  @example { rel: 'submit' }
     *
     *  2. Override default if the link rel 'submit' has name attribute use that for display
     *  @example { rel: 'submit', name: "Search" }
     *
     * In terms of form values:
     * ========================
     *
     *  1. In the case of POST, start with a new object and fill out values (based on the form)
     *  2. In the case of PUT, clone a new object based on the  existing item (ie prepopulate) and
     *     update values (based on the form)
     *
     * In terms of where and how to send forms:
     * ========================================
     *
     * 1. Default verb is POST when 'submit' is present
     * @example { rel: 'submit', href:"https://example.com/collection/"}
     *
     * 2. PUT verb if no link rel 'submit' OR method='PUT'
     * @example { rel: 'self', href: 'http://example.com/some/form"} <-- no submit
     *
     * 3. Set verb when link rel 'method'  is explicitly set
     * @example { rel: 'submit', method: 'PUT', href:"https://example.com/item"}
     * @example { rel: 'submit', method: 'POST', href:"https://example.com/collection"}
     *
     * 4. send to uri in named href if explicit
     * @example { rel: 'submit', href:"https://example.com/collection/"}
     *
     * 5. send to referring resource if omitted
     * @example { rel: 'self', href: 'http://example.com/some/form"} <-- no submit
     * @example { rel: 'submit'}
     *
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
            this.formObj = this.hasSubmitLinkRel(this.formRepresentation) ?
                {} :                                        // POST clean/new
                Object.assign({}, this.representation);     // PUT clone
        },
        computed: {
            /**
             * Defaults to 'Submit' and is overriden by the link relation 'submit' name value
             * @example { rel: 'submit', name: 'Search', href: ...
             * @type {string}
             */
            submitTitle() {
                // KLUDGE: only support one/first submit link rel d
                const [link, ..._] = SemanticLink.filter(this.formRepresentation, /^submit$/);
                return (link || {}).title || 'Submit';
            }
        },
        methods: {
            /**
             * @param {FormRepresentation} form
             * @return {boolean}
             */
            hasSubmitLinkRel(form) {
                return SemanticLink.matches(form, /^submit$/);
            },
            /**
             * On all the fields are entered then either make a PUT (edit/update) or POST (create, search) based on
             * the referring representation
             */
            submit() {

                this.onSubmit();

                /**
                 * A form will POST if there is a submit link rel
                 * A form will PUT if no sbmit
                 * A form will override above if a method is specified
                 * @param {FormRepresentation} form
                 * @return {string}
                 **/
                function verb(form) {
                    const [weblink, ..._] = SemanticLink.filter(form, /^submit$/);
                    if (weblink) {
                        return (weblink || {}).method || 'post';
                    } else {
                        return 'put';
                    }
                }

                const rel = this.hasSubmitLinkRel(this.formRepresentation) ? 'submit' : 'self';
                const links = this.hasSubmitLinkRel(this.formRepresentation) ? this.formRepresentation : this.representation;
                const putOrPost = verb(this.formRepresentation);

                return link[putOrPost](links, rel, 'application/json', this.formObj)
                    .then(/** @type {AxiosResponse} */response => {

                        if (this.onSuccess) {
                            this.onSuccess(response);
                        } else {

                            this.$notify({
                                text: `${response.statusText} <a href="${response.headers.location}">item<a>`,
                                type: 'success'
                            });

                            // on success if updated return to item (204) otherwise go to new (201)
                            // scope it here otherwise SemanticLink looks to loose scope (??) in the setTimeout
                            const returnUri = response.status === 201
                                ? response.headers.location
                                : SemanticLink.getUri(links, rel);

                            setTimeout(() => {
                                window.location = returnUri;
                            }, 4000)

                        }

                    })
                    .catch(/** @type {AxiosResponse} */response => {
                        if (this.onFailure) {
                            this.onFailure(response)
                        } else {
                            this.$notify({text: response.statusText, type: 'error'});
                        }

                    });
            },
            mapApiToUiType: mapApiToUiType
        }
    }
</script>