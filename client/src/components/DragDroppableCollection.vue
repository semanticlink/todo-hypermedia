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
            v-if="hasPatchWithJsonPatch"
            :model="representation"
            media-type="text/uri-list"
            :dropped="addAsPatch"
    >
        <b-button size="sm" variant="primary">Patch</b-button>
    </drag-and-droppable-model>
</span>
</template>

<script>

    import DragAndDroppableModel from './DragAndDroppableModel.vue';

    import {matches, get, put, getUri, link} from 'semantic-link';
    import {log} from 'logger';
    import {fromUriList, makeUriList} from "../lib/util/dragAndDropModel";

    import {compare, deepClone} from 'fast-json-patch'

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
            hasPatchWithJsonPatch() {
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
                    .catch(/** @type {AxiosError} */error => {
                        this.$notify({text: error.statusText, type: 'error'});
                    })
            },
            /**
             * Update the collection by adding a new uri to the existing items via Json Patch Document and put back to the
             * 'submit' rel on the form (or the collection if not present)
             * @param {string} document uri-list of the resources to be added to the collection
             */
            addAsPatch: function (document) {

                log.debug(`Adding uri-list: ${document}`);

                const newCollection = deepClone(this.representation);

                // add to the collection
                fromUriList(document).forEach(uri => newCollection.items.push({id: uri}));

                const patch = compare(this.representation, newCollection);

                get(this.representation, 'edit-form', 'application/json-patch+json')
                    .then(response => {
                        return link(response.data, 'submit', 'application/json-patch+json', 'PATCH', patch /*JSON.stringify(patch)*/);
                    })
                    .then(response => {
                        this.$notify({
                            text: `${response.statusText} <a href="${response.headers.location}">item<a>`,
                            type: 'success'
                        });

                    })
                    .catch(/** @type {AxiosError} */error => {
                        this.$notify({text: error.statusText, type: 'error'});
                    })
            }
        }
    };
</script>

<style scoped>

</style>