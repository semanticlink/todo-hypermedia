<template>

    <li :class="{ completed: item.completed, editing: editing }">

        <input class="toggle"
               type="checkbox"
               v-model="editTodo.completed">
        <input class="edit"
               type="text"
               v-model.trim="editTodo.name"
               v-item-focus="editing"
               @keyup.enter="doneEdit"
               @keyup.esc="resetTodo">
        <button class="save" @click="doneEdit"></button>


        <div class="view">
            <input class="toggle"
                   type="checkbox"
                   v-model="item.completed"
                   @click.prevent="updateCompleted">
            <label @dblclick="startEdit">{{ item.name }}</label>
            <button class="destroy" @click="removeTodo"></button>
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
     *   [x] Text [update]
     *
     * There are actually a number of states that need to be managed:
     *
     * 1. Update completed (without updating name)
     * 1a. Commit occurs on button press
     *
     * 2. Update name and/or completed together (ie you can move back-and-forth between the fields before commiting)
     * 2a. Commit occurs via either enter or save (+)
     * 2b. TODO: Commit occurs on blur off either of the name/commited fields
     *
     */

    import { log, nodMaker } from 'semanticLink';

    export default {
        props: {
            collection: { type: Object },
            item: { type: Object }
        },
        data() {
            return {
                editTodo: {},
                editing: false
            };
        },
        methods: {

            resetTodo() {
                this.editing = false;
                this.editTodo = {};
            },

            startEdit() {
                this.editing = true;
                this.editTodo = Object.assign({}, this.item);
            },

            removeTodo() {
                return nodMaker.deleteCollectionItem(this.collection, this.item);
            },

            doneEdit() {

                if (!this.editTodo) {
                    return;
                }

                /**
                 * Fancy delete. If you remove the name, it means you want it removed. Note: it is already trimed.
                 */
                if (!this.editTodo.name) {
                    this.removeTodo();
                }

                return nodMaker.updateResource(this.item, this.editTodo)
                    .then(() => this.resetTodo())
                    .catch(err => log.error(err));
            },

            updateCompleted() {
                this.startEdit();
                this.editTodo.completed = !this.editTodo.completed;
                this.doneEdit();
            }

        },

        // a custom directive to wait for the DOM to be updated
        // before focusing on the input field.
        // http://vuejs.org/guide/custom-directive.html
        directives: {
            'item-focus': (el, binding) => {
                if (binding.value) {
                    el.focus()
                }
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
