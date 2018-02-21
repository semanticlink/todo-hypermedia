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

            <section class="main" v-show="todos.length" v-cloak>
                <input class="toggle-all" type="checkbox" v-model="allDone">
                <ul class="todo-list">
                    <li v-for="todo in filteredTodos"
                        class="todo"
                        :key="todo.id"
                        :class="{ completed: todo.completed, editing: todo._inprogress }">

                        <div class="view">
                            <input class="toggle" type="checkbox" v-model="todo.completed" @change="completeTodo(todo)">
                            <label @dblclick="editTodo(todo)">{{ todo.name }}</label>
                            <button class="destroy" @click="removeTodo(todo)"></button>
                        </div>

                        <!-- document version to be edited -->
                        <input class="toggle"
                               type="checkbox"
                               v-if="todo._inprogress"
                               v-model="todo._inprogress.completed">
                        <input class="edit"
                               type="text"
                               v-if="todo._inprogress"
                               v-model.trim="todo._inprogress.name"
                               v-todo-focus="todo._inprogress !== null"
                               @blur="doneEdit(todo)"
                               @keyup.enter="doneEdit(todo)"
                               @keyup.esc="cancelEdit(todo)">
                    </li>
                </ul>
            </section>

            <footer class="footer" v-show="todos.length" v-cloak>

                <span class="todo-count">
                  <strong>{{ remaining }}</strong> {{ remaining | pluralize }} left
                </span>

                <ul class="filters">
                    <li><a v-on:click="showAll" :class="{ selected: visibility == 'all' }">All</a></li>
                    <li><a v-on:click="showActive" :class="{ selected: visibility == 'active' }">Active</a></li>
                    <li><a v-on:click="showCompleted" :class="{ selected: visibility == 'completed' }">Completed</a>
                    </li>
                </ul>

                <button class="clear-completed" @click="removeCompleted" v-show="todos.length > remaining">
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
    import { redirectToTenant } from "../router";

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
        props: {
            apiUri: { type: String },
        },
        data() {
            return {
                todoCollection: {},
                todos: [],
                newTodo: DEFAULT_TODO,
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
                return filters[this.visibility](this.todos)
            },
            remaining: function () {
                return filters.active(this.todos).length
            },
            allDone: {
                get: function () {
                    return this.remaining === 0
                },
                set: function (value) {
                    this.todos.forEach(todo => this.completeTodo(todo, value))
                }
            }
        },

        methods: {
            setEditing(todo, edit) {
                this.$set(todo, '_inprogress', Object.assign({}, edit || todo))
            },
            unsetEditing(todo) {
                this.$set(todo, '_inprogress', null);
                delete todo._inprogress;
            },

            addTodo: function (todoDocument, todoCollection) {
                nodMaker.createCollectionResourceItem(todoCollection, Object.assign({}, todoDocument))
                    .then(item => {
                        todoDocument = this.DEFAULT_TODO;
                        return this.unsetEditing(item);
                    })
                    .catch(err => log.error(err));
            },

            removeTodo: function (todo) {
                nodMaker.deleteCollectionItem(this.todoCollection, todo);
            },

            completeTodo: function (todo) {
                // this gets the updated version and does a force PUT (I think)
                nodMaker.updateResource(todo, Object.assign({}, todo));
            },

            editTodo: function (todo) {
                this.setEditing(todo);
            },

            doneEdit: function (todo) {

                const editedTodo = todo._inprogress;

                if (!editedTodo) {
                    return;
                }

                /**
                 * Fancy delete. If you remove the name, it means you want it removed. Note: it is already trimed.
                 */
                if (!editedTodo.name) {
                    this.removeTodo(todo);
                }

                const vm = this;
                vm.unsetEditing(todo);
                nodMaker.updateResource(todo, editedTodo)
                    .catch(err => {
                        vm.setEditing(todo, editedTodo);
                        log.error(err);
                    });
            },

            cancelEdit: function (todo) {
                this.unsetEditing(todo);
            },

            removeCompleted: function () {
                filters.completed(this.todos).forEach(todo => this.removeTodo(todo));
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

                getTenant(this.$root.$api, this.apiUri)
                    .then(tenant => redirectToTenant(tenant, query));


            }
        },


        // a custom directive to wait for the DOM to be updated
        // before focusing on the input field.
        // http://vuejs.org/guide/custom-directive.html
        directives: {
            'todo-focus':

                function (el, binding) {
                    // if (binding.value) {
                    el.focus()
                    // }
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
