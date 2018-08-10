<template>

    <li :class="{ completed: item.completed, editing: editing }" :id="self">

        <input class="toggle"
               type="checkbox"
               v-model="editItem.completed">
        <input class="edit"
               type="text"
               v-model.trim="editItem.name"
               @keyup.enter="doneEdit"
               @keyup.esc="reset">
        <button class="save" @click="doneEdit"></button>


        <div class="view">
            <input class="toggle"
                   type="checkbox"
                   v-model="item.completed"
                   @click.prevent="updateCompleted">
            <label @dblclick="startEdit">{{ item.name }}</label>
            <button class="destroy" @click="remove"></button>
        </div>


    </li>
</template>

<script>

    /**
     *
     * This component is a line item that allows the user to update:
     *
     *  * completed (checkbox)
     *  * name (label)
     *
     * The wireframe layout is:
     *
     *   [x <&circle-x>] Text [update <&plus>]
     *
     * There are actually a number of states that need to be managed:
     *
     * 1. Update completed (without updating name)
     * 1a. Commit occurs on button press
     *
     * 2. Update name and/or completed together (ie you can move back-and-forth between the fields before committing)
     * 2a. Commit occurs via either enter or save (+)
     * 2b. TODO: Commit occurs on blur off either of the name/committed fields
     *
     *
     */

    import * as cache from 'semanticLink/cache';
    import * as link from 'semantic-link';
    import {log} from 'logger';
    import {mapCompletedToState, mapStateToCompleted} from "../lib/form-type-mappings";

    export default {
        props: {
            collection: {type: Object, required: true},
            item: {type: Object, required: true}
        },
        data() {
            return {
                editItem: {},
                editing: false
            };
        },
        computed: {
            self() {
                // Evaluating the item actually helps with binding. Without this you'll find that items won't bind properly
                return link.getUri(this.item, /self/);
            }
        },
        methods: {

            /**
             * Clear the editing mode back to view
             */
            reset() {
                this.editing = false;
                this.editItem = {};
            },

            /**
             * Make the field editable with the latest values
             */
            startEdit() {
                this.editing = true;
                this.editItem = {...this.item};
            },

            /**
             * Take the edited values and update if there is a change and delete in the case that because
             * the name has been emptied there is a logical delete. Flush these changes back through the collection
             * server-side
             */
            doneEdit() {

                if (!this.editItem) {
                    return;
                }

                // Fancy delete. If you remove the name, it means you want it removed. Note: it is already trimmed.
                if (!this.editItem.name) {
                    this.remove();
                }

                // Fancy ui needs to make bool --> state (yuck)
                this.editItem.state = mapCompletedToState(this.editItem.completed);

                return cache.updateResource(this.item, this.editItem)
                // really we should get a copy from the server
                    .then(() => this.item.completed = mapStateToCompleted(this.editItem.state))
                    .then(() => this.reset())
                    .catch(err => log.error(err));
            },

            /**
             * Flush the delete back through the collection server-side
             */
            remove() {
                return cache.deleteCollectionItem(this.collection, this.item);
            },

            /**
             * Update the specific field 'completed' in one go and rebind
             */
            updateCompleted() {
                this.startEdit();
                this.editItem.completed = !this.editItem.completed;
                this.doneEdit();
            }

        }
    };
</script>

<style>


    .todo-list li .save {
        display: none;
        position: absolute;
        top: 0;
        right: 10px;
        bottom: 0;
        width: 40px;
        height: 40px;
        margin: auto 0;
        font-size: 30px;
        color: #cc9a9a;
        margin-bottom: 11px;
        transition: color 0.2s ease-out;
    }

    .todo-list li .save:after {
        content: '+';
    }

    .todo-list li.editing .save {
        display: block;
        cursor: hand;
    }

</style>
