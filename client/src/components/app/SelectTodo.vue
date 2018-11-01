<template>
    <div class="hello">
        <h1>Todo lists</h1>

        <ul v-for="todo in todos.items" v-cloak>

            <li><b-link @click="gotoTodo(todo)">{{ todo.name }}</b-link></li>

        </ul>
    </div>
</template>

<script>
    import {log} from 'logger';
    import {redirectToTodo} from "router";
    import DragAndDroppableModel from '../DragAndDroppableModel.vue'
    import bButton from 'bootstrap-vue/es/components/button/button';
    import {getTodoList} from 'domain/todo';

    export default {
        components: {DragAndDroppableModel, bButton},
        data() {
            return {
                msg: 'Looking ...',
                error: false,
                invalid: '',
                busy: true,
                todos: {},
            };
        },
        created: function () {

            log.info(`Loading selected todo from root api`);

            /**
             * Based on an authenticated user
             *   - if they have only one list then show that list (redirect to the todos)
             *   - if they have none, make then create one
             *   - if they have multiple then make them select one
             *
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            const strategyOneProvidedTodo = apiResource => {

                log.info('Looking for existing todos');

                return getTodoList(apiResource)
                    .then(todos => {

                        const [head, ...tail] = (todos || {}).items || [];

                        if (head.length === 0) {
                            this.$notify({
                                title: "You have no todo lists and need to create some",
                                type: 'info'
                            });

                            // TODO: implement create in the UI

                        } else if (tail.length === 0) {
                            // there is only one todo so let's go there
                            return this.gotoTodo(head)
                        }
                        // else, load up the tenants for the user to select (currently sparsely populated)
                        this.todos = todos;
                    })
                    .catch(err => {
                        this.$notify({
                            title: response.statusText,
                            text: err,
                            type: 'error'
                        });
                        log.error(err);
                    });
            };

            return strategyOneProvidedTodo(this.$root.$api);

        },
        methods: {
            /**
             *
             * @param {TodoRepresentation} todoList a top level todo list
             */
            gotoTodo(todoList) {
                redirectToTodo(todoList)
            },
        },
    };
</script>
