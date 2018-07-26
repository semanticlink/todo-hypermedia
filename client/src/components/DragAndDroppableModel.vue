<template>
        <span draggable="true"
              @drop="drop"
              @dragstart="dragstart"
              @dragover="dragover"
              @dragenter="dragenter"
              @dragleave="dragleave"
              @dragend="dragend">
                <slot>
                        <span class="btn btn-xs btn-success glyphicon glyphicon-import"
                              role="button">

                        </span>
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
             * Optional callback function to load up the model on drag start
             * @return {Promise.<LinkedRepresentation>}
             */
            dragStart: {
                type: Function,
                default: () => Promise.resolve()
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
            drop(event) {
                return drop(
                    event,
                    document => this.dropped(document, this.context),
                    this.mediaType);
            },
            dragstart(event) {
                this.dragStart()
                    .then(resource => {

                        const model = resource ? resource : this.model;

                        if (this.mediaType === undefined) {
                            log.warn('No mediaType set using application/json');
                        }
                        return dragstart(event, model, this.mediaType || 'application/json');
                    })
            },
            dragover,
            dragenter,
            dragleave,
            dragend
        }
    };
</script>