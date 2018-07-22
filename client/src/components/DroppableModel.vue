<template>
    <span
            @drop="drop"
            @dragstart="dragstart"
            @dragover="dragover"
            @dragenter="dragenter"
            @dragleave="dragleave"
            @dragend="dragend">
    >
        <slot>
            <span class="btn btn-xs btn-success glyphicon glyphicon-import"
                  role="button">
            </span>
        </slot>
    </span>
</template>x

<script>

    import {dragend, dragenter, dragleave, dragover, drop} from '../lib/util/dragAndDropModel';

    export default {
        name: 'droppable-model',
        props: {
            /**
             * In-memory {@link LinkedRepresentation} that a dropped in the context of another {@link LinkedRepresentation}
             */
            model: {
                type: Object,
                required: true
            },
            /**
             * Callback after the dropped model is loaded and then can be acted on (eg updated)
             */
            dropped: {
                type: Function,
                default: () => () => {
                }
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
            drop: function (event) {
                return drop(
                    event,
                    representation => this.dropped(representation, this.model),
                    this.mediaType);
            },
            dragover,
            dragenter,
            dragleave,
            dragend
        }
    };
</script>