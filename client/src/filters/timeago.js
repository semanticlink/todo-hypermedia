import {_} from 'underscore';

/**
 * modified from http://jsfiddle.net/i_woody/cnL5T/
 *
 * TODO: A self updating version is here: http://jsfiddle.net/nolandubeau/NQ6mA/
 *
 * TODO: probably this should just wrap Moment because we already have it
 */
export default function timeago (input, p_allowFuture) {

    if (!input) {
        return '';
    }
    let substitute = function (stringOrFunction, number, strings) {
            const string = _(stringOrFunction).isFunction()
                ? stringOrFunction(number, dateDifference)
                : stringOrFunction;
            const value = (strings.numbers && strings.numbers[number]) || number;
            return string.replace(/%d/i, value);
        },
        nowTime = (new Date()).getTime(),
        date = (new Date(input)).getTime(),
        //refreshMillis= 6e4, //A minute
        allowFuture = p_allowFuture || true,
        strings = {
            prefixAgo: null,
            prefixFromNow: null,
            suffixAgo: 'ago',
            suffixFromNow: 'from now',
            second: '%d seconds',
            seconds: 'less than a minute',
            minute: 'about a minute',
            minutes: '%d minutes',
            hour: 'about an hour',
            hours: 'about %d hours',
            day: 'a day',
            days: '%d days',
            month: 'about a month',
            months: '%d months',
            year: 'about a year',
            years: '%d years'
        },
        dateDifference = nowTime - date,
        words,
        seconds = Math.abs(dateDifference) / 1000,
        minutes = seconds / 60,
        hours = minutes / 60,
        days = hours / 24,
        years = days / 365,
        separator = strings.wordSeparator === undefined ? ' ' : strings.wordSeparator,

        // var strings = this.settings.strings;
        prefix = strings.prefixAgo,
        suffix = strings.suffixAgo;

    if (allowFuture) {
        if (dateDifference < 0) {
            prefix = strings.prefixFromNow;
            suffix = strings.suffixFromNow;
        }
    }

    words = seconds < 20 && substitute(strings.second, Math.round(seconds), strings) ||
        seconds < 45 && substitute(strings.seconds, Math.round(seconds), strings) ||
        seconds < 90 && substitute(strings.minute, 1, strings) ||
        minutes < 45 && substitute(strings.minutes, Math.round(minutes), strings) ||
        minutes < 90 && substitute(strings.hour, 1, strings) ||
        hours < 24 && substitute(strings.hours, Math.round(hours), strings) ||
        hours < 42 && substitute(strings.day, 1, strings) ||
        days < 30 && substitute(strings.days, Math.round(days), strings) ||
        days < 45 && substitute(strings.month, 1, strings) ||
        days < 365 && substitute(strings.months, Math.round(days / 30), strings) ||
        years < 1.5 && substitute(strings.year, 1, strings) ||
        substitute(strings.years, Math.round(years), strings);

    return [prefix, words, suffix].join(separator);

}
