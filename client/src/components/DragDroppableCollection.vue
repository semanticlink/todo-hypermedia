<template>
<span>
    <drag-and-droppable-model
            v-if="hasPutWithUriList"
            :model="representation"
            media-type="text/uri-list"
            :dropped="addAsPut"
    >
        <b-button size="sm" variant="primary">Put uri-list</b-button>
    </drag-and-droppable-model>

    <drag-and-droppable-model
            v-if="hasPatchWIthJsonPatch"
            :model="representation"
            media-type="application/json-patch+json"
    >
        <b-button size="sm" variant="primary">Patch</b-button>
    </drag-and-droppable-model>
</span>
</template>

<script>

    import DragAndDroppableModel from './DragAndDroppableModel.vue';

    import {matches, get, put, getUri} from 'semantic-link';
    import {log} from 'logger';
    import {fromUriList, makeUriList} from "../lib/util/dragAndDropModel";

    export default {
        name: 'drag-droppable-collection',
        components: {DragAndDroppableModel},
        props: {
            /**
             * Collection or item representation.
             * @type {CollectionRepresentation|LinkedRepresentation}
             */
            representation: {
                type: Object,
                required: true
            },
        },
        computed: {
            hasPutWithUriList() {
                return matches(this.representation, 'edit-form', 'text/uri-list');
            },
            hasPatchWIthJsonPatch() {
                return matches(this.representation, 'edit-form', 'application/json-patch+json');
            }
        },
        methods: {
            /**
             * Update the collection by adding a new Uri to the existing item uri-list and put back to the
             * 'submit' rel on the form (or the collection if not present)
             * @param {string} document uri-list of the resources to be added to the collection
             */
            addAsPut: function (document) {

                log.debug(`Adding uri-list: ${document}`);

                const itemUris = (this.representation.items || []).map(({id}) => id);
                const putUriList = makeUriList([...new Set([...itemUris, ...fromUriList(document)])]);

                get(this.representation, 'edit-form', 'text/uri-list')
                    .then(response => {
                        return put(response.data, 'submit', 'text/uri-list', putUriList);
                    })
                    .then(response => {
                        this.$notify({
                            text: `${response.statusText} <a href="${response.headers.location}">item<a>`,
                            type: 'success'
                        });

                    })
                    .catch(/** @type {AxiosError} */response => {
                        this.$notify({text: response, type: 'error'});
                    })
            }
        }
    };
</script>

<style scoped>

</style>