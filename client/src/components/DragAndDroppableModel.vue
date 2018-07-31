<template>
    <!-- this is a draggable span but the draggable is added when the model is loaded
         this is to get around async with sync events
         -->
    <span @mousedown="mousedown"
          @mouseup="mousedown"
          @click="mousedown"
          @drop="drop"
          @dragstart="dragstart"
          @dragover="dragover"
          @dragenter="dragenter"
          @dragleave="dragleave"
          @dragend="dragend">
                <slot>
                        <span>+</span>
                </slot>
        </span>
</template>

<script>

    import {dragend, dragenter, dragleave, dragover, dragstart, drop} from '../lib/util/dragAndDropModel';
    import {log} from '../lib/logger';

    export default {
        name: 'drag-and-droppable-model',
        props: {
            /**
             * In-memory model that can be dragged out. This can either be a {@link LinkedRepresentation} or a
             * {@link Promise.<LinkedRepresentation>}
             */
            model: {
                type: Object | Function,
                required: true
            },
            /**
             * In-memory {@link LinkedRepresentation} that a dropped in the context of another {@link LinkedRepresentation}
             */
            context: {
                type: Object,
                default: () => {
                    return {};
                }
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
        data() {
            return {
                resource: {}
            }
        },
        methods: {
            mousedown(event) {

                /**
                 * Work around to load up the model from async before making it draggable
                 */
                const getModel = (typeof this.model === 'object')
                    ? () => Promise.resolve(this.model)
                    : this.model;

                getModel()
                    .then(resource => {
                        this.resource = resource;
                        event.target.setAttribute('draggable', true)
                    })

            },
            drop(event) {
                return drop(
                    event,
                    document => this.dropped(document, this.context),
                    this.mediaType);
            },
            dragstart(event) {

                if (this.mediaType === undefined) {
                    log.warn('No mediaType set using application/json');
                }

                dragstart(event, this.resource, this.mediaType || 'application/json');

            },
            dragover,
            dragenter,
            dragleave,
            dragend
        }
    };
</script>

<style>
    /*
    https://medium.com/@reiberdatschi/common-pitfalls-with-html5-drag-n-drop-api-9f011a09ee6c
     */
    .drag * {
        pointer-events: none;
    }
</style>