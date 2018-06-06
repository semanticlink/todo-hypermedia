import { fragmentToRoam } from '../router';
import linkifyHtml from 'linkifyjs/html';

/**
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
 * Take an in-memory object and pretty print JSON with clickable links doing client view mapping to
 * the router
 * @param {*} obj
 * @param {number=4} spaces
 * @returns {*}
 */
const linkifyWithClientRouting = (obj, spaces) => linkHighlightWithClientRouting(syntaxHighlight(JSON.stringify(obj, null, spaces || 4)));

/**
 * Take an in-memory object and pretty print JSON with clickable links.
 * @param {*} obj
 * @param {number=4} spaces
 * @returns {*}
 */
const linkifyToSelf = (obj, spaces) => linkHighlightToSelf(syntaxHighlight(JSON.stringify(obj, null, spaces || 4)));

export { linkifyToSelf, linkifyWithClientRouting };
export default linkifyWithClientRouting;