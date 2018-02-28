<template>
    <div>

        <section class="todoapp">

            <header class="header">
                <h1>todos</h1>
                <input class="new-todo"
                       autofocus autocomplete="off"
                       placeholder="What needs to be done?"
                       v-model="newTodo.name"
                       @keyup.enter="addTodo">
            </header>

            <section class="main" v-show="todoCollection.items.length" v-cloak>
                <input class="toggle-all" type="checkbox" v-model="allDone">
                <ul class="todo-list">
                    <todo-item v-for="todo in filteredTodos"
                               class="todo"
                               :item="todo"
                               :collection="todoCollection">

                    </todo-item>
                </ul>
            </section>

            <footer class="footer" v-show="todoCollection.items.length" v-cloak>

                <span class="todo-count">
                  <strong>{{ remaining }}</strong> {{ remaining | pluralize }} left
                </span>

                <ul class="filters">
                    <li><a v-on:click="showAll" :class="{ selected: visibility == 'all' }">All</a></li>
                    <li><a v-on:click="showActive" :class="{ selected: visibility == 'active' }">Active</a></li>
                    <li><a v-on:click="showCompleted" :class="{ selected: visibility == 'completed' }">Completed</a>
                    </li>
                </ul>

                <button class="clear-completed" @click="removeCompleted"
                        v-show="todoCollection.items.length > remaining">
                    Clear completed
                </button>
            </footer>

        </section>

        <footer class="info">
            <p>Double-click to edit a todo</p>
            <p>Based on <a href="http://evanyou.me">Evan You</a></p>
            <p>Part of <a href="http://todomvc.com">TodoMVC</a></p>
        </footer>

    </div>
</template>

<script>

    import { _, nodMaker } from 'semanticLink';
    import { log } from 'logger';
    import { redirectToTenant } from 'router';
    import TodoItem from './TodoItem.vue';
    import { getTenant, getTodos, DEFAULT_TODO } from "domain/todo";

    /**
     * This component displays and allows updates to the todo list
     *
     * The wireframe layout is:
     *
     * [] toggle all completed |  "new item" [enter]
     *
     * + <TodoItem> <-- nested component
     * + <TodoItem>
     *
     * "items" left | (x) All | () Active | () Completed | [Clear Completed]
     *
     * Basic usage:
     *
     * Filters to display: active, completed, all
     * Update: toggle all for between completed and not completed
     * Delete: remove all that are completed
     *
     * Filters usage:
     *
     * The filters have a client-side state that is contained in the URL query params.
     *
     * @example http://todo.example.com/#/todo/a/tenant/1?completed
     *
     *   This shows (filters) the completed (and loaded) todos client side
     *
     */

        // visibility filters
    const filters = {
            all: todos => todos,
            active: todos => todos.filter(todo => !todo.completed),
            completed: todos => todos.filter(todo => todo.completed)
        };

    /**
     * Enum for visibility client-side state
     * @readonly
     * @enum {string}
     */
    const filterEnum = {
        ALL: 'all',
        ACTIVE: 'active',
        COMPLETED: 'completed'
    };


    export default {
        components: { TodoItem },
        props: {
            apiUri: { type: String },
        },
        data() {
            return {
                todoCollection: { items: [] },
                newTodo: Object.assign({}, DEFAULT_TODO),
                visibility: filterEnum.ALL,
                filter: filterEnum
            };
        },

        filters: {
            pluralize: function (n) {
                return n === 1 ? 'item' : 'items'
            }
        },

        created: function () {

            /**
             * Visibility filter can be handed in via the Uri (eg http://localhost:8080/#/todo/a/tenant/1?completed)
             * @type {filterEnum}
             */
            const visibilityFilterFromUriQuery = _(this.$route.query).chain().keys().first().value();
            this.setVisibilityFilter(visibilityFilterFromUriQuery);

            log.debug(`Loading selected organisation`);

            return getTodos(this.$root.$api, this.apiUri)
                .then(todos => {
                    this.todoCollection = todos;
                    return this.todos = this.todoCollection.items;
                })
                .catch(err => log.error(err));

        },
        computed: {
            filteredTodos: function () {
                return filters[this.visibility](this.todoCollection.items)
            },
            remaining: function () {
                return filters.active(this.todoCollection.items).length
            },
            allDone: {
                get: function () {
                    return this.remaining === 0
                },
                set: function (value) {
                    this.todoCollection.items.forEach(todo => {
                        const updateTodo = Object.assign({}, todo, { completed: value });
                        nodMaker.updateResource(todo, updateTodo)
                            .catch(err => log.error(err));
                    })
                }
            }
        },

        methods: {

            /**
             * Clears out the new todo reaady to be entered and created
             */
            resetTodo() {
                this.newTodo = Object.assign({}, this.newTodo, DEFAULT_TODO);
            },

            /**
             * Adds the new todo document into the existing todo collection
             */
            addTodo: function () {
                return nodMaker.createCollectionResourceItem(this.todoCollection, Object.assign({}, this.newTodo))
                    .then(todoResource => nodMaker.getResource(todoResource)
                        .then(() => this.resetTodo()))
                    .catch(err => log.error(err));
            },

            /**
             * Iterates through all the completed todos and deletes them from the collection. This is a (server-side) API delete
             * rather than just a client-side filter.
             */
            removeCompleted: function () {
                filters.completed(this.todoCollection.items).forEach(todo => nodMaker.deleteCollectionItem(this.todoCollection, todo));
            },

            // FILTERS



            showAll: function () {
                this.redirectOnVisibilityChange(filterEnum.ALL);
            },
            showActive: function () {
                this.redirectOnVisibilityChange(filterEnum.ACTIVE);
            },
            showCompleted: function () {
                this.redirectOnVisibilityChange(filterEnum.COMPLETED);
            },

            /**
             * Filter todos based on state
             * @param {filterEnum} filter
             */
            setVisibilityFilter: function (filter) {
                switch (filter) {
                    case filterEnum.COMPLETED:
                        this.visibility = filterEnum.COMPLETED;
                        break;
                    case filterEnum.ACTIVE:
                        this.visibility = filterEnum.ACTIVE;
                        break;
                    case filterEnum.ALL:
                    default:
                        this.visibility = filterEnum.ALL;
                        break;
                }
            },

            /**
             * Ensure that the filter state on client-side and uri match
             * @param {filterEnum} filter
             */
            redirectOnVisibilityChange: function (filter) {

                this.setVisibilityFilter(filter);

                let query = {};
                switch (filter) {
                    case filterEnum.COMPLETED:
                        query = { query: { completed: '' } };
                        break;
                    case filterEnum.ACTIVE:
                        query = { query: { active: '' } };
                        break;
                    case filterEnum.ALL:
                    default:
                        break;
                }

                return getTenant(this.$root.$api, this.apiUri)
                    .then(tenant => redirectToTenant(tenant, query));


            }
        }
    };
</script>

<style>
    [v-cloak] {
        display: none;
    }
</style>
