<template>
<span>
    <drag-and-droppable-model
            v-if="hasLinkRel"
            :model="representation"
            :media-type="mediaType"
            :accept="accept"
            :dropped="update">

            <FormAction :media-type="mediaType" :rel="rel" :type="method" :title="title"/>

    </drag-and-droppable-model>

</span>
</template>

<script>

    import DragAndDroppableModel from './DragAndDroppableModel.vue';
    import FormAction from './FormAction.vue';

    import * as link from 'semantic-link';

    /**
     * A thin wrapper over the drag and drop making it specifics to PUT or PATCH on collections
     */
    export default {
        name: 'drag-droppable-collection',
        components: {DragAndDroppableModel, FormAction},
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
             * @type {MediaType}
             */
            mediaType: {
                type: String,
                required: true
            },
            /**
             * @type {RelationshipType}
             */
            rel: {
                type: String,
                default: 'edit-form'
            },
            /**
             * The title of the tooltip.
             */
            title: {
                type: String,
                default: ''
            },
            /**
             * Http verbs (eg PUT, PATCH, GET, POST, DELETE)
             */
            method: {
                type: String,
                required: true
            },
            /**
             * This function transforms the representation into the microformat/media type (eg uri-list or
             * Json Patch Document) required for the verb (PUT, PATCH)
             */
            map: {
                type: Function,
                default: () => m => m
            },
            /**
             * @type {MediaType}
             */
            accept: {
                type: String
            }
        },
        computed: {
            hasLinkRel() {
                return link.matches(this.representation, this.rel, this.mediaType);
            }

        },
        methods: {
            /**
             * Update the collection by adding a new Uri to the existing item uri-list and put back to the
             * 'submit' rel on the form (or the collection if not present)
             * Update the collection by adding a new uri to the existing items and put back to the
             * 'submit' rel on the form (or the collection if not present)
             *
             * The 'map' function in the parent provides the  Json Patch Document or uri-list
             */
            update(data) {
                const uriList = this.map(data, this.representation);
                link.get(this.representation, this.rel, this.mediaType)
                    .then(response => link[this.method.toLowerCase()](response.data, 'submit', this.mediaType, uriList))
                    .then(/** @type {AxiosResponse} */response => {
                        this.$notify({
                            text: `${response.statusText} <a href="${response.headers.location}">item<a>`,
                            type: 'success'
                        });

                    })
                    .catch(/** @type {AxiosError} */error => {
                        this.$notify({text: error.message || error.statusText, type: 'error'});
                    })
            }
        }
    };
</script>

<style scoped>

</style>