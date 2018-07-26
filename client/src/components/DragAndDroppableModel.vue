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
        methods: {
            drop(event) {
                return drop(
                    event,
                    document => this.dropped(document, this.context),
                    this.mediaType);
            },
            dragstart(event) {

                // get the model in its hydrated form
                const dragStart = (typeof this.model === 'object')
                    ? Promise.resolve(this.model)
                    : this.model;

                dragStart()
                    .then(resource => {

                        if (this.mediaType === undefined) {
                            log.warn('No mediaType set using application/json');
                        }
                        return dragstart(event, resource, this.mediaType || 'application/json');
                    })
            },
            dragover,
            dragenter,
            dragleave,
            dragend
        }
    };
</script>