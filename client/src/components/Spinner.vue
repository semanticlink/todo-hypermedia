<template>

    <span v-if="loading" class="whole-page">
        <simple-spinner
                :animation-duration="1000"
                :size="40"
                :color="'#ff1d5e'"
                :message="message"/>
        <b-link @click="cancel">Cancel</b-link>
    </span>


</template>

<script>

    import {loader, LoaderEvent} from 'semantic-network';
    import bLink from 'bootstrap-vue/src/components/link/link';
    import SimpleSpinner from 'vue-simple-spinner/src/components/Spinner.vue'
    import _ from 'underscore';

    /**
     * This is a simple work-in-progress demonstration of attached a "load centre".
     *
     * In large data sets for provisioning, you need somewhere to monitor all the network and be
     * able to cancel. For example, provisioning data can be 100K lines of JSON with up to thousand or so requests
     * queued.
     *
     * This design is simple:
     *
     *  - show a loader if there is loader 'work' (centered on the page)
     *  - give the queue size if it is large
     *  - add a cancel action
     *
     *  Also, we only update the spinner at a human readable rate (eg 200 milliseconds)
     *
     *  TODO: make setting props if required
     *  TODO: inline version (next to an element rather than centre to the page)
     */
    export default {
        name: "Spinner",
        components: {SimpleSpinner, bLink},
        data() {
            return {
                itemsQueued: 0
            }
        },
        computed: {
            loading() {
                return this.itemsQueued > 0;
            },
            message() {
                return this.loading && this.itemsQueued > 30
                    ? `Remaining ${this.itemsQueued}`
                    : ''
            },
            showCancel() {
                return this.itemsQueued > 2;
            }
        },
        created() {

            loader.limiter.on(LoaderEvent.DEBUG, () => {
                this.itemsQueued = 1;
                // let's not update too often
                _.debounce(this.readQueues, 200, true);
            });
            loader.limiter.on(LoaderEvent.IDLE, () => {
                this.itemsQueued = 0;
            })
        },
        methods: {
            readQueues() {
                /**
                 * @see {@link Bottleneck.Counts}
                 */
                const {RECEIVED, RUNNING, EXECUTING, QUEUED} = loader.limiter.counts();
                this.itemsQueued = RECEIVED + QUEUED + RUNNING + EXECUTING;
            },
            cancel() {
                loader.clearAll()
                    .then(() => {
                        this.itemsQueued = 0;
                    });
            }
        }
    }
</script>

<style scoped>

    .whole-page {
        position: fixed;
        z-index: 999;
        height: 2em;
        width: 2em;
        overflow: show;
        margin: auto;
        top: 0;
        left: 0;
        bottom: 0;
        right: 0;
    }
</style>