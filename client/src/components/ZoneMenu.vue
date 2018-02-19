<template>
    <div class="zone-menu">
        <div class="label-wrapper" @click="toggleChildren">
            <div :style="indent" :class="labelClasses">

                <i v-if="nodes" class="fa" :class="iconClasses"></i>
                {{ label }}

                <div v-if="nodes.zones">
                    <zone-menu
                            v-if="showChildren"
                            v-for="(node, index) in nodes.zones.items"
                            :key="node.name"
                            :nodes="node"
                            :label="node.name"
                            :type="'zone'"
                            :depth="depth + 1"
                    >
                    </zone-menu>
                </div>

                <div v-if="nodes.zonegroups">
                    <zone-menu
                            v-for="(node, index) in nodes.zonegroups.items"
                            :key="node.name"
                            :nodes="node"
                            :label="node.name"
                            :type="'zonegroup'"
                            :depth="depth + 1"
                    >
                    </zone-menu>

                </div>
            </div>

        </div>
    </div>
</template>

<script>
    export default {
        props: ['label', 'nodes', 'depth', 'type'],
        name: 'zone-menu',
        data () {
            return {
                showChildren: true
            };
        },
        computed: {
            iconClasses () {
                return {
                    'fa-plus-square-o': !this.showChildren,
                    'fa-minus-square-o': this.showChildren
                };
            },
            labelClasses () {
                return {'has-children': this.nodes};
            },
            indent () {
                return {transform: `translate(${this.depth * 50}px)`};
            }
        },
        methods: {
            toggleChildren () {
                this.showChildren = !this.showChildren;
            }
        }
    };
</script>

<style lang="scss" scoped>

    .zone-menu {
        .label-wrapper {
            padding-bottom: 10px;
            margin-bottom: 10px;
            border-bottom: 1px solid #ccc;

            .has-children {
                cursor: pointer;
            }
        }
    }

</style>