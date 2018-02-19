<template>
    <span draggable="true" @dragstart="dragstart" @dragend="dragend">{{ msg || '' }}</span>
</template>

<script>

    import { dragend, dragstart } from '../lib/util/dragAndDropModel';

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