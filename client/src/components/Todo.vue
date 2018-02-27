<template>
    <div>

        <section class="todoapp">

            <header class="header">
                <h1>todos</h1>
                <input class="new-todo"
                       autofocus autocomplete="off"
                       placeholder="What needs to be done?"
                       v-model="newTodo.name"
                       @keyup.enter="addTodo(newTodo, todoCollection)">
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

    import { _, log, nodMaker, SemanticLink } from 'semanticLink';
    import { redirectToTenant } from '../router';
    import TodoItem from './TodoItem.vue';

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

    /**
     * use the organisation from a provided list (when authenticated)
     * @param {ApiRepresentation} apiResource
     * @param {string} tenantUri
     * @returns {Promise<TenantRepresentation>}
     */
    const getTenant = (apiResource, tenantUri) => {
        return nodMaker
            .getResource(apiResource)
            .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /tenants/))
            .then(tenants => nodMaker.getCollectionResourceItemByUri(tenants, tenantUri));
    };

    /**
     * use the organisation from a provided list (when authenticated)
     * @param {ApiRepresentation} apiResource
     * @param {string} tenantUri
     * @returns {Promise<TodoCollectionRepresentation>}
     */
    const getTodos = (apiResource, tenantUri) => {
        log.debug(`Looking for todos in tenant ${tenantUri}`);

        return getTenant(apiResource, tenantUri)
            .then(tenant => nodMaker.getNamedCollectionResourceAndItems(tenant, 'todos', /todos/));
    };

    /**
     * Default values for a todo item. This *could/should* be retrieved from a create form on the collectin
     * @type {TodoRepresentation}
     */
    const DEFAULT_TODO = { name: '', completed: false };

    export default {
        components: {
            todoItem: TodoItem
        },
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

            resetTodo() {
                this.newTodo = Object.assign({}, this.newTodo, DEFAULT_TODO);
            },

            addTodo: function (todo, todoCollection) {
                return nodMaker.createCollectionResourceItem(todoCollection, Object.assign({}, todo))
                    .then(todoResource => nodMaker.getResource(todoResource)
                        .then(() => this.resetTodo()))
                    .catch(err => log.error(err));
            },

            removeCompleted: function () {
                filters.completed(this.todoCollection.items).forEach(todo => nodMaker.deleteCollectionItem(this.todoCollection, todo));
            },

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
