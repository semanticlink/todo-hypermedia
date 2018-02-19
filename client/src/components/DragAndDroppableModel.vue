<template>
        <span draggable="true"
              @drop="drop"
              @dragstart="dragstart"
              @dragover="dragover"
              @dragenter="dragenter"
              @dragleave="dragleave"
              @dragend="dragend"
              class="btn btn-xs btn-success glyphicon glyphicon-import"
              role="button">
        </span>
</template>

<script>

    import { dragend, dragenter, dragleave, dragover, dragstart, drop } from '../lib/util/dragAndDropModel';
    import { log, SemanticLink } from 'semanticLink';

    export default {
        name: 'drag-and-droppable-model',
        props: {
            /**
             * In-memory {@link LinkedRepresentation} that can be dragged out
             */
            model: {
                type: Object,
                required: true
            },
            /**
             * In-memory {@link LinkedRepresentation} that a dropped in the context of another {@link LinkedRepresentation}
             */
            context: {
                type: Object,
                default: () => { return {};}
            },
            /**
             * Callback after the dropped model is loaded and then can be acted on (eg updated)
             */
            dropped: {
                type: Function,
                default: () => () => {}
            }
        },
        methods: {
            drop: function (event) {
                return drop(event, document => {
                    log.debug(`[Dropped] document: ${SemanticLink.tryGetUri(document, /self/)}`);
                    this.dropped(document, this.context);
                });
            },
            dragstart: function (event) {
                return dragstart(event, this.model);
            },
            dragover,
            dragenter,
            dragleave,
            dragend
        }
    };
</script>

<style scoped>

</style>