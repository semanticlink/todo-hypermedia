/**
 * This was originally written as an alert store for an angular app
 *
 * TODO: port up to other frameworks eg React/Vue (or abandon)
 */
export default class Alert {

    /**
     * @param context bind the presentation-layer context to this store
     * @param $timeout each framework will have their own way to trigger updates
     * @param {number} defaultTimeout=5000
     */
    constructor (context, $timeout, defaultTimeout = 5000) {
        this.context = context;
        this.$timeout = $timeout;
        this.defaultTimeout = defaultTimeout;
        this.clear();
    }

    /**
     *
     * @param {Error|string} msg
     * @param type
     * @param timeout
     * @return {*}
     */
    add (msg, type = Alert.success, timeout = this.defaultTimeout) {
        msg = msg.message || msg;

        let alert = {
            type: type,
            msg: msg,
            close: () => {
                this.context.alerts.splice(this.context.alerts.indexOf(this), 1);
            }
        };
        if (type === Alert.success) {
            this.$timeout(() => {
                this.context.alerts.splice(this.context.alerts.indexOf(alert), 1);
            }, timeout);
        }
        return this.context.alerts.push(alert);
    }

    /**
     *
     * @param {Error|string} msg
     * @param timeout
     * @return {*}
     */
    error (msg, timeout) {
        return this.add(msg, Alert.error, timeout);
    }

    /**
     *
     * @param {Error|string} msg
     * @param timeout
     * @return {*}
     */
    success (msg, timeout) {
        return this.add(msg, Alert.success, timeout);
    }

    /**
     *
     * @param {Error|string} msg
     * @param timeout
     * @return {*}
     */
    info (msg, timeout) {
        return this.add(msg, Alert.info, timeout);
    }

    /**
     *
     * @param {Error|string} msg
     * @param timeout
     * @return {*}
     */
    warning (msg, timeout) {
        return this.add(msg, Alert.warning, timeout);
    }

    closeAlert (alert) {
        return this.closeAlertIdx(this.context.alerts.indexOf(alert));
    }

    closeAlertIdx (index) {
        return this.context.alerts.splice(index, 1);
    }

    clear () {
        this.context.alerts = [];
    }

    get () {
        return this.context.alerts;
    }
}

Alert.success = 'success';
Alert.info = 'info';
Alert.warning = 'warning';
Alert.error = 'danger';
