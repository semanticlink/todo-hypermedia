<template>
    <div>

        <section class="todoapp">

            <header class="header">
                <h1>todos</h1>
                <input class="new-todo"
                       autofocus autocomplete="off"
                       placeholder="What needs to be done?"
                       v-model="newTodo"
                       @keyup.enter="addTodo">
            </header>

            <section class="main" v-show="todos.length" v-cloak>
                <input class="toggle-all" type="checkbox" v-model="allDone">
                <ul class="todo-list">
                    <li v-for="todo in filteredTodos"
                        class="todo"
                        :key="todo.id"
                        :class="{ completed: todo.completed, editing: todo == editedTodo }">
                        <div class="view">
                            <input class="toggle" type="checkbox" v-model="todo.completed">
                            <label @dblclick="editTodo(todo)">{{ todo.name }}</label>
                            <button class="destroy" @click="removeTodo(todo)"></button>
                        </div>
                        <input class="edit" type="text"
                               v-model="todo.name"
                               v-todo-focus="todo == editedTodo"
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

    const filterEnum = {
        ALL: 'all',
        ACTIVE: 'active',
        COMPLETED: 'completed'
    };

    export default {
        props: {
            apiUri: { type: String },
        },
        data() {
            return {
                todoCollection: {},
                tenant: {},
                todos: [],
                newTodo: '',
                editedTodo: null,
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

            switch (_(this.$route.query).chain().keys().first().value()) {
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

            log.debug(`Loading selected organisation`);

            /**
             * use the organisation from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            const getTodos = (apiResource) => {
                log.debug('Looking for tenant in', this.apiUri);

                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /tenants/))
                    .then(tenants => nodMaker.getCollectionResourceItemByUri(tenants, this.apiUri))
                    .then(tenant => {
                        this.tenant = tenant;
                        return nodMaker.getNamedCollectionResourceAndItems(tenant, 'todos', /todos/);
                    })
                    .then(todos => {
                        this.todoCollection = todos;
                        return this.todos = this.todoCollection.items;
                    })
                    .catch(err => log.error(err));
            };

            return getTodos(this.$root.$api);

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
                    this.todos.forEach(todo => {
                        todo.completed = value
                    })
                }
            }
        },

        methods: {
            addTodo: function () {
                const value = this.newTodo && this.newTodo.trim();
                if (!value) {
                    return
                }
                this.todos.push({
                    id: todoStorage.uid++,
                    title: value,
                    completed: false
                });
                this.newTodo = '';
            },

            removeTodo: function (todo) {
                this.todos.splice(this.todos.indexOf(todo), 1);
            },

            editTodo: function (todo) {
                this.beforeEditCache = todo.title;
                this.editedTodo = todo;
            },

            doneEdit: function (todo) {
                if (!this.editedTodo) {
                    return;
                }
                this.editedTodo = null;
                todo.title = todo.title.trim();
                if (!todo.title) {
                    this.removeTodo(todo);
                }
            },

            cancelEdit: function (todo) {
                this.editedTodo = null;
                todo.title = this.beforeEditCache;
            },

            removeCompleted: function () {
                this.todos = filters.active(this.todos);
            },
            showAll: function () {
                this.visibility = filterEnum.ALL;
                redirectToTenant(this.tenant);
            },
            showActive: function () {
                this.visibility = filterEnum.ACTIVE;
                redirectToTenant(this.tenant, { query: { active: '' } });
            },
            showCompleted: function () {
                this.visibility = filterEnum.COMPLETED;
                redirectToTenant(this.tenant, { query: { completed: '' } });
            }
        },


        // a custom directive to wait for the DOM to be updated
        // before focusing on the input field.
        // http://vuejs.org/guide/custom-directive.html
        directives: {
            'todo-focus': function (el, binding) {
                if (binding.value) {
                    el.focus()
                }
            }
        }
    };
</script>

<style>
    [v-cloak] {
        display: none;
    }
</style>
