<template>
    <span draggable="true" @dragstart="dragstart" @dragend="dragend">
        <slot>{msg}</slot>
    </span>
</template>

<script>

    import {dragend, dragstart} from '../lib/util/dragAndDropModel';

    export default {
        name: 'draggable-model',
        props: {
            msg: String,
            /**
             * In-memory {@link LinkedRepresentation} that can be dragged out
             */
            model: {
                type: Object,
                required: true
            },
            /**
             * Pick the type of media to return
             * @example 'application/json'
             * @example 'text/uri-list'
             * @example 'text/plain'
             * @example 'DownloadUrl'
             */
            mediaType: {
                type: String
            }
        },
        methods: {
            dragstart: function (event) {
                return dragstart(event, this.model);
            },
            dragend
        }
    };
</script>

<style scoped>

</style>