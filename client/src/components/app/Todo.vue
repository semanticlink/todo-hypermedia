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

            <section class="main" v-show="totalItems" v-cloak>
                <input class="toggle-all btn" type="checkbox" v-model="allDone">
                <ul class="todo-list">
                    <!-- Using index as key to avoid warning -->
                    <todo-item v-for="(todo, index) in filteredTodos"
                               :key="index"
                               class="todo"
                               :item="todo"
                               :collection="todoCollection">

                    </todo-item>
                </ul>
            </section>

            <footer class="footer" v-show="totalItems" v-cloak>

                <span class="todo-count">
                  <strong>{{ remaining }}</strong> {{ remaining | pluralize }} left
                </span>

                <ul class="filters">
                    <li><a v-on:click="showAll" class="btn" :class="{ selected: isAll() }">All</a></li>
                    <li><a v-on:click="showActive" class="btn" :class="{ selected: isActive() }">Active</a></li>
                    <li><a v-on:click="showCompleted" class="btn" :class="{ selected: isCompleted() }">Completed</a>
                    </li>
                </ul>

                <button class="clear-completed"
                        @click="removeAllCompleted"
                        v-show="totalItems > remaining">
                    Clear completed
                </button>
            </footer>

        </section>

        <footer class="info">
            <p>Double-click to edit a todo</p>
            <p>Written by the <a href="https://github.com/semanticlink">semantic link project</a>
                and originally based on <a href="http://todomvc.com/examples/vue/">Vue.js</a> version by
                <a href="http://evanyou.me">Evan You</a></p>
            <p>Part of <a href="http://todomvc.com">TodoMVC</a></p>
        </footer>

    </div>
</template>

<script>

    import {del, get, create, update} from 'semantic-network';
    import {log} from 'logger';
    import {redirectToTodo} from 'router';
    import TodoItem from './TodoItem.vue';
    import {defaultTodo, getTodoListByUri, getTodoListAndItemsByUri, getTodos} from "domain/todo";
    import {mapCompletedToState} from "domain/form-type-mappings";

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
        components: {TodoItem},
        props: {
            apiUri: {type: String},
        },
        data() {
            return {
                /**
                 * Holds a reference to the collection for processing and is bound to the screen. Note: we need
                 * an early binding to 'items' for it to be reactive.
                 *
                 * @type TodoCollectionRepresentation
                 */
                todoCollection: {},

                /**
                 * New item holder - this will be initiated in created
                 */
                newTodo: {name: ""},

                /**
                 * Client-side filtering display that gets set from the incoming URL query: defaults {@link filterEnum.All}
                 */
                visibility: filterEnum.ALL
            };
        },

        filters: {
            pluralize: n => n === 1 ? 'item' : 'items'
        },

        created() {

            log.debug(`Loading selected todo ${this.apiUri}`);

            /**
             * Visibility filter can be handed in via the Uri (eg http://localhost:8080/#/todo/a/tenant/1?completed)
             * @type {filterEnum}
             */
            const visibilityFilterFromUriQuery = Object.keys(this.$route.query)[0];
            this.setVisibilityFilter(visibilityFilterFromUriQuery);

            return getTodoListAndItemsByUri(this.$root.$api, this.apiUri)
                .then(todos => this.todoCollection = todos)
                .then(todos => getTodos(todos))
                .then(this.reset)
                .catch(log.error);

        },
        computed: {
            filteredTodos() {
                return filters[this.visibility](this.todoItems)
            },
            remaining() {
                return filters[filterEnum.ACTIVE](this.todoItems).length;
            },
            totalItems() {
                return this.todoItems.length;
            },
            todoItems() {
                return this.todoCollection.items || [];
            },
            allDone: {
                get() {
                    return this.remaining === 0
                },
                set(value) {

                    // process all the todoItems and toggle the completed fields
                    return update(
                        this.todoItems,
                        /**
                         * @type {TodoRepresentation}
                         */
                        todo => {
                            /**
                             * One of the issues with representations is that many fields are optional (eg completed).
                             * In such cases, we need to ensure that Vue is bound to them.
                             *
                             * In many ways, it actually shows the domain smell because we provided completed as
                             * a helper property that is a rule of a subset of state.
                             */
                            todo.completed = this.$set(todo, 'completed', value);

                            return {...todo, state: mapCompletedToState(value)};
                        },
                        this.$root.options)
                        .catch(log.error);
                }
            }
        },

        methods: {

            /**
             * Clears out the new todo ready to be entered and created
             */
            reset() {
                this.newTodo = {...defaultTodo(this.todoCollection)};
            },

            /**
             * Adds the new todo document into the existing todo collection
             */
            addTodo() {
                return create(this.todoCollection, {where: {...this.newTodo}})
                    .then(todoResource => get(todoResource)
                        .then(this.reset))
                    .catch(log.error);
            },

            /**
             * Clear Completed: iterates through all the completed todos and deletes them from the collection.
             * This is a (server-side) API delete rather than just a client-side filter.
             */
            removeAllCompleted() {
                return Promise
                    .all(filters[filterEnum.COMPLETED](this.todoCollection.items)
                        .map(todo => del(this.todoCollection, {where: todo})))
                    .catch(log.error);
            },

            // **********************************
            // FILTERS
            // **********************************

            /**
             * Filter todos based on state (usually incoming from URL query param)
             * @param {filterEnum} filter
             */
            setVisibilityFilter(filter) {
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

            isAll() {
                return this.visibility === filterEnum.ALL;
            },
            isActive() {
                return this.visibility === filterEnum.ACTIVE;
            },
            isCompleted() {
                return this.visibility === filterEnum.COMPLETED;
            },

            // Change filters

            showAll() {
                this.redirectOnVisibilityChange(filterEnum.ALL);
            },
            showActive() {
                this.redirectOnVisibilityChange(filterEnum.ACTIVE);
            },
            showCompleted() {
                this.redirectOnVisibilityChange(filterEnum.COMPLETED);
            },


            /**
             * Ensure that the filter state on client-side and uri match
             * @param {filterEnum} filter
             */
            redirectOnVisibilityChange(filter) {

                this.setVisibilityFilter(filter);

                let query = {};
                switch (filter) {
                    case filterEnum.COMPLETED:
                        query = {query: {completed: ''}};
                        break;
                    case filterEnum.ACTIVE:
                        query = {query: {active: ''}};
                        break;
                    case filterEnum.ALL:
                    default:
                        break;
                }

                return getTodoListByUri(this.$root.$api, this.apiUri)
                    .then(todo => redirectToTodo(todo, query));

            }
        }
    }
    ;
</script>

<style>
    [v-cloak] {
        display: none;
    }
</style>
