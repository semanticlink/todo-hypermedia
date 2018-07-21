import {fragmentToRoam} from '../router';
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

/**
 * Customise linkify so that create client-side fragments for the href and don't create new tabs
 *
 * @example
 *
 *      with link: http://localhost:5000/todo/1
 *      clients-die view: /roam/a
 *
 *      before: <a href="http://localhost:5000/todo/1" target="_blank">http://localhost:5000/todo/1<a>
 *      after:  <a href="#/roam/a/todo/1">http://localhost:5000/todo/1<a>
 *
 * Note: using the fragment allows us to ignore the client scheme:[//authority]path[?query]
 *
 * @param val
 * @returns {*}
 */
const linkHighlightWithClientRouting = val => linkifyHtml(val, {
    // ensure the the attribute target is removed otherwise we create new tabs in the browser
    target: {
        url: undefined
    },
    // construct a client-side fragment to remain with the browser
    formatHref: fragmentToRoam
});

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
 * Take an in-memory object and pretty print JSON with clickable links doing client view mapping to
 * the router
 * @param {*} obj
 * @param {number=4} spaces
 * @returns {*}
 */
// const linkifyWithClientRouting = (obj, spaces) => linkHighlightWithClientRouting(syntaxHighlight(stringify(obj, spaces));
const linkifyWithClientRouting = (obj, spaces) => linkHighlightWithClientRouting(syntaxHighlight(humanise(obj, spaces)));

/**
 * Take an in-memory object and pretty print JSON with clickable links.
 * @param {*} obj
 * @param {number=4} spaces
 * @returns {*}
 */
// const linkifyToSelf = (obj, spaces) => linkHighlightToSelf(syntaxHighlight(stringify(obj, spaces)));
const linkifyToSelf = (obj, spaces) => linkHighlightToSelf(syntaxHighlight(humanise(obj, spaces)));

export {linkifyToSelf, linkifyWithClientRouting};
export default linkifyWithClientRouting;