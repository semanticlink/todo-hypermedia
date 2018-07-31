<template>


    <!-- wrap the icon in a button with an optional tooltip -->
    <b-button
            size="sm"
            variant="outline"
            @click="onClick(rel,mediaType)"
            v-b-tooltip.hover.html.right
            :title="title"
            :show.sync="showToolTip">

        <Delete :h="h" :w="w" v-if="type === 'Delete'"/>
        <Add :h="h" :w="w" v-else-if="type === 'Add'"/>
        <Edit :h="h" :w="w" v-else-if="type === 'Edit'"/>
        <Search :h="h" :w="w" v-else-if="type === 'Search'"/>
        <Put :h="h" :w="w" v-else-if="type === 'Put'"/>
        <Patch :h="h" :w="w" v-else="type === 'Path'"/>

    </b-button>

</template>

<script>

    import Delete from 'vue-ionicons/dist/md-remove-circle-outline.vue';
    import Add from 'vue-ionicons/dist/md-add-circle-outline.vue';
    import Edit from 'vue-ionicons/dist/md-create.vue';
    import Search from 'vue-ionicons/dist/md-search.vue';
    import Put from 'vue-ionicons/dist/md-cloud-upload.vue';
    import Patch from 'vue-ionicons/dist/md-cloud-upload.vue';
    import bButton from 'bootstrap-vue/es/components/button/button'

    import bTooltip from 'bootstrap-vue/es/components/tooltip/tooltip'


    export default {
        name: "Form",
        components: {Delete, Add, Edit, Search, Put, Patch, bButton, bTooltip},
        props: {
            onClick: {
                type: Function,
                default: () => {
                }
            },
            rel: {default: 'edit-form'},
            mediaType: {default: ''},
            type: {default: 'Edit'},
            /**
             * The title of the tooltip.
             */
            title: {
                type: String,
                default: ''
            },

        },
        computed: {
            showToolTip() {
                return this.title !== '';
            }
        },
        data() {
            return {
                w: "22px",
                h: "22px"
            }
        }
    }
</script>