import linkifyHtml from 'linkifyjs/html';
import {DateTime} from 'luxon';

/**
 * Pretty print the Json in html. Additionally strip noisy syntax (eg quotes) and reformat times to be readable.
 *
 * @see https://stackoverflow.com/questions/4810841/how-can-i-pretty-print-json-using-javascript
 * @param {string} json
 * @returns {*}
 */
function syntaxHighlight(json) {
    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        let cls = 'number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'key';
            } else {
                cls = 'string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'boolean';
        } else if (/null/.test(match)) {
            cls = 'null';
        }

        match = match
        // remove quotes around all keys
            .replace(/^"(.*)":$/, '$1:')
            // remove quotes around values
            .replace(/^"(.*)"$/, '$1');

        const date = DateTime.fromISO(match);

        if (date.isValid) {
            match = date.toLocaleString(DateTime.DATETIME_HUGE_WITH_SECONDS);
        }

        return '<span class="' + cls + '">' + match + '</span>';
    });
}

const linkHighlightToSelf = val => linkifyHtml(val, {
    // ensure the the attribute target is removed otherwise we create new tabs in the browser
    target: {
        url: undefined
    }
});


/**
 * Stringifies with identing and removing noisy commas so that non devs can more easily read it
 *
 * @see https://jsfiddle.net/DerekL/mssybp3k/
 * @param obj
 * @param spaces
 * @returns {string}
 */
const humanise = (obj, spaces) => JSON
    .stringify(obj, null, spaces || 4)
    .replace(/([}\]"]),/g, '$1');

/**
 * Take an in-memory object and pretty print JSON with clickable links.
 * @param {*} obj
 * @param {number=4} spaces
 * @returns {*}
 */
export const linkifyToSelf = (obj, spaces) => linkHighlightToSelf(syntaxHighlight(humanise(obj, spaces)));

