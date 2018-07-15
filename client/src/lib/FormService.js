import { _ } from 'semanticLink';
import axios from 'axios';
import { getAuthenticationUri, getBearerLinkRelation } from '../lib/http-interceptors';
import { filter, matches, post, put, get } from 'semantic-link';

/**
 * A form has the job to POST to a collection or PUT to an item (this is by convention).
 *
 * The semantics of the form are that:
 *
 * In terms of the display label:
 * ==============================
 *
 *  1. Default value is 'Submit'
 *  @example { rel: 'submit' }
 *
 *  2. Override default if the link rel 'submit' has name attribute use that for display
 *  @example { rel: 'submit', name: "Search" }
 *
 * In terms of form values:
 * ========================
 *
 *  1. In the case of POST, start with a new object and fill out values (based on the form)
 *  2. In the case of PUT, clone a new object based on the  existing item (ie prepopulate) and
 *     update values (based on the form)
 *
 * In terms of where and how to send forms:
 * ========================================
 *
 * 1. Default verb is POST when 'submit' is present
 * @example { rel: 'submit', href:"https://example.com/collection/"}
 *
 * 2. PUT verb if no link rel 'submit' OR method='PUT'
 * @example { rel: 'self', href: 'http://example.com/some/form"} <-- no submit
 *
 * 3. Set verb when link rel 'method'  is explicitly set
 * @example { rel: 'submit', method: 'PUT', href:"https://example.com/item"}
 * @example { rel: 'submit', method: 'POST', href:"https://example.com/collection"}
 *
 * 4. send to uri in named href if explicit
 * @example { rel: 'submit', href:"https://example.com/collection/"}
 *
 * 5. send to referring resource if omitted
 * @example { rel: 'self', href: 'http://example.com/some/form"} <-- no submit
 * @example { rel: 'submit'}
 *
 */
export default class FormService {

    /**
     * @param {FormRepresentation} form
     * @return {boolean}
     */
    static hasSubmitLinkRel(form) {
        return matches(form, /^submit$/);
    }

    /**
     *
     * @param {*} data
     * @param {FormRepresentation}form
     * @param {CollectionRepresentation} collection
     * @returns {Promise<AxiosResponse<any>>}
     */
    static submitForm(data, form, collection) {
        /**
         * A form will POST if there is a submit link rel
         * A form will PUT if no submit
         * A form will override above if a method is specified
         * @param {FormRepresentation} form
         * @return {string}
         **/
        function verb(form) {
            const [weblink] = filter(form, /^submit$/);
            if (weblink) {
                return /*(weblink || {}).method ||*/ post;
            } else {
                return put;
            }
        }

        /**
         * Only send back the fields from the data that are matched to the fields on the form
         */
        function pickFieldsFromForm(data, form) {

            const fields = form.items.map(item => item.name);
            return _(data).pick(fields);
        }

        const rel = FormService.hasSubmitLinkRel(form) ? 'submit' : 'self';
        const links = FormService.hasSubmitLinkRel(form) ? form : collection;
        const putOrPost = verb(form);
        const obj = pickFieldsFromForm(data, form);

        return putOrPost(links, rel, 'application/json', obj);
    }

    /**
     *
     * Uses the www-authenticate header to load up the collection and the create form
     * ready for submission back to the api
     *
     * @param {AxiosError} error 401 trapped error with www-authenticate header
     * @returns {Promise<AxiosResponse<LinkedRepresentation[]>>}
     */
    static loadFormFrom401BearerChallenge(error) {
        return axios.get(getAuthenticationUri(error))
            .then(response => get(response.data, getBearerLinkRelation(error)))
            .then(authenticateCollection => {
                return get(authenticateCollection.data, /create-form/)
                    .then(authenticateLoginRepresentation => {
                        return [
                            authenticateLoginRepresentation.data,
                            authenticateCollection.data
                        ];
                    });
            });
    }

    /**
     * When we show a form on the screen, decide whether to clone or create an in-memory representation
     * @param {FormRepresentation} form
     * @param {CollectionRepresentation} collection
     * @returns {*}
     */
    static makeFormObj(form, collection) {
        return FormService.hasSubmitLinkRel(form)
            ? {}                                 // POST clean/new
            : Object.assign({}, collection);     // PUT clone
    }
}