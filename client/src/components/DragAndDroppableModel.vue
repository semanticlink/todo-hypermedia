<template>
    <!-- this is a draggable span but the draggable is added when the model is loaded
         this is to get around async with sync events
         -->
    <span
            ref="draggable"
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

    import {dragend, dragenter, dragleave, dragover, dragstart, drop} from 'semantic-link-utils/dragAndDropModel';
    import {log} from 'logger';
    import {eventBus} from "semantic-link-utils/EventBus";

    export default {
        name: 'drag-and-droppable-model',
        props: {
            /**
             * In-memory model that can be dragged out. This can either be a {@link LinkedRepresentation} or a
             * {@link Promise.<LinkedRepresentation>}
             */
            model: {
                type: Object | Function,
                required: false
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
             *
             * Takes a callback function with the dropped document as the parameter
             */
            dropped: {
                type: Function,
                default: () => (document) => document
            },
            /**
             * Pick the type of media to send from a drag event
             *
             * // TODO: is this really a content-type (cf accept)
             * @example 'application/json'
             * @example 'text/uri-list'
             * @example 'text/plain'
             * @example 'DownloadUrl'
             */
            mediaType: {
                type: String
            },
            /**
             * Pick the type of media to return from a drop event
             *
             * @example 'application/json'
             * @example 'text/uri-list'
             * @example 'text/plain'
             * @example 'DownloadUrl'
             */
            accept: {
                type: String,
            },
            async: {
                type: Boolean,
                required: false,
                default: () => false
            }
        },
        mounted() {
            if (this.async) {
                const vm = this;
                eventBus.$on('resource:ready', () => {
                    vm.$refs.draggable.setAttribute('draggable', true);
                })
            } else {
                this.$refs.draggable.setAttribute('draggable', true);
            }
        },
        methods: {
            drop(event) {

                if (this.accept === undefined) {
                    log.debug('No accept media type set using application/json');
                }

                drop(
                    event,
                    document => this.dropped(document, this.context),
                    this.accept || 'application/json');
            },
            dragstart(event) {

                if (this.mediaType === undefined) {
                    log.debug('No mediaType set using application/json');
                }

                dragstart(event, this.model, this.mediaType || 'application/json');

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