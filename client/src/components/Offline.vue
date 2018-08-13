<template>
    <transition name="slideup">
        <div class="offline-indicator" v-if="offline">
            <h1 slot="header">API not responding</h1>
            <div slot="body">
                <div>It looks like somebody has broken the internet ... please be patient while we fix it</div>
                <div>Last seen: {{ lastseen | timeago }}</div>
                <div>Checking again in: {{ waitFor | timeago }}</div>
            </div>
        </div>
    </transition>
</template>

<script>
    import {eventBus} from 'semantic-link-utils/EventBus';
    import {offline, restored, checking} from 'semantic-link-utils/authEvent';
    import axios from 'axios';
    import {httpQueue} from 'semantic-link-utils/HTTPQueue';
    import timeago from '../filters/timeago';

    /**
     * This is a simple bool 'lock'. When the event is triggered we don't
     * want to flood the user with waiting dialogs, rather just present one and block
     * them from continuing.
     *
     * This method could instead create a singleton style promise that is returned
     * to all callers. The first call which would create the promise would service
     * and resolve that promise for all subsequent callers.
     */
    let isPerformingHttpRestore;

    /**
     * Offline:
     *    - waits for the offline event (triggered by no network)
     *    - then it retries the network request at increasing intervals (backoff strategy)
     *    - as it waits it tells the user when the api was last seen and when the next requset will be made
     *    - once the network returns it closes the message and replays any queued requests
     */
    export default {
        name: 'Offline',
        filters: {
            timeago
        },
        data() {
            return {
                /**
                 * false = don't show anything
                 * true = show the alert/banner
                 */
                offline: false,
                /**
                 * time in milliseconds
                 */
                interval: 50,
                /**
                 * the date that we base the last seen calculation off.
                 *
                 * TODO: check that this gets refreshed if the API goes down a second time
                 */
                lastvisit: new Date()
            };
        },
        computed: {
            /**
             * Relative time before the next request will be made
             * @return {number} time in milliseconds
             */
            waitFor: function () {
                let now = new Date();
                return now.setMilliseconds(now.getMilliseconds() + this.interval);
            },
            /**
             * Relative time since the Api was last seen as up
             * @return {number} time in milliseconds
             */
            lastseen: function () {
                let now = new Date();
                return now.setSeconds(this.lastvisit.getSeconds());
            }
        },
        mounted() {
            eventBus.$on(offline, this.updateOnlineStatus);
        },
        methods: {
            updateOnlineStatus(error) {
                this.offline = true;

                this.checkOffline(this.interval, error.config);

                // back off so that we don't overload
                this.interval = this.interval * 2;

                if (isPerformingHttpRestore) {
                    return;
                }
                isPerformingHttpRestore = true;
                this.offline = false;
                eventBus.$emit(restored);
            },
            checkOffline(interval, config) {
                const vm = this;

                eventBus.$emit(checking, interval);

                // after the interval retry talking to the api if it works, retry all (dequeue)
                // we process the error object rather than deal with a Promise.reject that would
                // allow us to process as a 'catch' TODO: check: using reject/catch didn't appear to work
                return setTimeout(
                    () => axios(config)
                        .then(error => {
                            if (error && error.message !== 'Network Error') {
                                vm.offline = false;
                                eventBus.$emit(restored);
                                httpQueue.retryAll();
                            }
                        }),
                    interval);
            }
        }
    };
</script>

<style scoped>
    .offline-indicator {
        background: #E8EDE9;
        border-radius: 3px;
        border-bottom: 2px solid #d3d9d4;
        padding: 15px 5px;
        text-align: center;
        width: 100%;
        margin: 5px 0;
        font-family: sans-serif;
        color: #202126;
        box-sizing: border-box;
    }

    .slideup-enter-active, .slideup-leave-active {
        transition: transform .2s, opacity .2s;
        transform: none;
        opacity: 1;
    }

    .slideup-enter, .slideup-leave-active {
        opacity: 0;
        transform: translateY(-100%);
    }
</style>