<template>
    <span @drop="drop"
          @dragover="dragover"
          @dragenter="dragenter"
          @dragleave="dragleave"
          class="btn btn-xs btn-success glyphicon glyphicon-plus"
          role="button">
    </span>
</template>

<script>

    import { log } from 'logger';
    import { dragend, dragenter, dragleave, dragover, drop } from '../lib/util/dragAndDropModel';

    export default {
        name: 'droppable-model',
        props: {
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
                return drop(event, representation => {
                    log.debug(representation);
                    this.dropped(representation, this.context);
                });
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