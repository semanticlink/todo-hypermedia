import {_, log} from 'semanticLink';

/**
 * Grok the extension of a file name
 * @param {string} file
 * @returns {string}
 */
const getFileExtension = (file) => file.name.replace(/.*\.([a-zA-Z]+)$/, '$1');

/**
 * Returns the model out of the dataTransfer in order of:
 *  * application/json
 *  * File (application/json) - first file only
 *
 *  This is a synchronise version
 *
 * @param {DataTransfer} transfer - drag event
 * @param {string} mediaType
 * @returns {LinkedRepresentation|Object|undefined}
 */
const getDragData = (transfer, mediaType) => {

    /**
     *
     * Warning on dataTransfer.types
     *
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#droptargets
     *
     * Note that the latest spec now dictates that DataTransfer.types should return a frozen array
     * of DOMStrings rather than a DOMStringList (this is supported in Firefox 52 and above.
     * As a result, the contains method no longer works on the property; the includes method
     * should be used instead to check if a specific type of data is provided, using code
     * like the following:
     *
     * @example
     *         if ([...event.dataTransfer.types].includes('text/html')) {
         *              // Do something
         *          }
     */

    log.debug(`[Drop] media type: [${mediaType}]`);
    log.debug(`[Drop] types available: [${[transfer.types].join(',')}]`);

    Object.entries(transfer.items).forEach(
        ([, item]) => log.debug(`[Drag] item: kind '${item.kind}' type '${item.type}'`)
    );

    if (mediaType === 'application/json' && [...transfer.types].includes('application/json')) {

        log.debug('[Drop] found: application/json');

        const data = transfer.getData('application/json');
        return JSON.parse(data);

    } else if (mediaType === 'text/uri-list' && _(transfer.items).any(item => item.kind === 'string' && item.type === 'text/uri-list')) {

        log.debug('[Drop] found: text/uri-list');

        let uriItem = _(transfer.items).find(item => item.kind === 'string' && item.type === 'text/uri-list');


        uriItem.getAsString(str => {
            log.debug(`[Drop] result: ${str}`);
            return str;
        });

    } else if (_(transfer.files).size() > 0) {

        // currently we are only going to take one file
        // TODO: multiple dropped files
        const file = _(transfer.files).first();
        log.debug(`File: "${file.name}" of type ${file.type}`);

        const extension = getFileExtension(file);
        // TODO: wider set of mime types
        if ('json' === extension) {

            /**
             * Apologies, this is a blocking, synchronise file reader
             */

            let ready = false;
            let result = '';

            const reader = new FileReader();
            reader.onload = function (evt) {
                log.debug('[Drop] data loaded');
                try {
                    result = JSON.parse(evt.target.result);
                    ready = true;
                } catch (err) {
                    log.error(err);
                    return undefined;
                }
            };

            reader.readAsText(file);

            const check = () => {
                if (ready === true) {
                    return result;
                }
                setTimeout(check, 500);
            };

            check();


        } else {
            log.error('Unknown file type');
            return undefined;
        }
    } else if (mediaType === 'text/plain' && [...transfer.types].includes('text/plain')) {
        //
        //  After all the content types and file have been processed, have a few guesses
        //  at what the content is. If it has come from an editor then JSON could be
        //  represented as text. Try parsing it as JSON and proceed if that works.
        //
        log.debug('[Drop] trying text as JSON');
        try {
            return JSON.parse(transfer.getData('Text'));
        } catch (e) {
            log.error('[Drop] error parsing text as JSON');
            if (e instanceof SyntaxError) {
                log.error('The content is not valid JSON')
                return undefined;
            }
            else {
                log.error('The content type is not JSON and unsupported')
                return undefined;
            }
        }
    } else {
        log.debug(`[Drop] content is an unsupported file/content type: '${mediaType}' in [${[transfer.types].join(',')}]`);
        return undefined;
    }


};

/**
 * Name of a file dragged off the browser if we can't find a title.
 * @type {string}
 */
const DEFAULT_FILE_NAME = 'Unnamed';

const makeJson = model => {

    // Alert: JSON.stringify can crash the browser on a large model with a complex replacer strategy
    //        If no strategy is handed in then we will use the canonical form
    try {
        return JSON.stringify(model, null, 0);

    } catch (e) {
        log.error(e);
        return JSON.stringify({}, null, 0);
    }
};

const makePrettyJson = (model, replacer) => {

    try {
        return JSON.stringify(model, replacer || null, 2);

    } catch (e) {
        log.error(e);
        return JSON.stringify({}, null, 1);
    }
};

/**
 * Sets up a data transfer. Currently works to transfer model as JSON only. Currently offers
 * up types:
 *
 * * File (DownloadURL) will name the file on title, description, or default Unnamed (in that order)
 * * text/plain
 * * application/json
 *
 * TODO: text/csv (for dragging to Excel)
 *
 * @param {LinkedRepresentation|Object} model
 * @param {Event} event
 * @param {function(key:string, value:string):string|undefined=} replacerStrategy JSON.stringify replacer function
 *         see https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify
 * @param {?string|string[]} mediaType media types to be included in the {@link DataTransfer} object on drag
 */
const setDragData = (model, event, replacerStrategy, mediaType) => {

    const dataTransfer = event.dataTransfer;

    mediaType = mediaType || ['application/json', 'text/plain', 'DownloadUrl', 'text/uri-list'];
    mediaType = [mediaType];

    log.debug(`[Drag] using media types: [${mediaType.join(',')}]`);


    if (mediaType.includes('application/json')) {

        dataTransfer.clearData('application/json');
        log.debug('[Drop] set data application/json');
        const data = makeJson(model);
        dataTransfer.setData('application/json', data);

    }

    if (mediaType.includes('DownloadUrl')) {
        log.debug('[Drop] set data DownloadURL');

        /**
         * the model should be {@link LinkedRepresentation} so there are search strategy. Title should actually
         * be retired.
         * TODO: we could use link relation 'self' to get titles, for example
         * @type {string}
         */
        const filename = (model.title || model.name || model.description || DEFAULT_FILE_NAME) + '.json';

        // encode the prettyJson so that carriage returns are not lost
        // http://stackoverflow.com/questions/332872/encode-url-in-javascript
        // 'DownloadURL' is chrome specific (I think) see https://www.html5rocks.com/en/tutorials/casestudies/box_dnd_download/
        dataTransfer.setData(
            'DownloadURL',
            `application/json:${filename}:data:application/json,${encodeURI(makePrettyJson(model, replacerStrategy))}`
        );
    }

    if (mediaType.includes('text/plain')) {
        dataTransfer.setData('text/plain', makePrettyJson(model, replacerStrategy));
        log.debug('[Drop] set data text/plain');
    }

    if (mediaType.includes('text/uri-list')) {
        /**
         * Making a uri-list is a subset of the rfc.
         *
         * DO NOT include any comments. It breaks the {@link DataTransfer} object
         */
        dataTransfer.setData('text/uri-list', makeUriList(model));
        log.debug('[Drop] set data text/uri-list');
    }


};

/**
 * The replacer strategy is a cleanser of object models to across-the-wire representations, such that when
 * we get models out they look close to what they look like when the arrive across-the-wire in the first place.
 * The default removes the edit and create forms links.
 *
 * The default replacer also has hidden functionality that if you hold down th control key, you can get back the
 * in-memory version without replacements. This is useful for debugging.
 *
 * @param event
 * @return {function(key:string, value:string):string|undefined}
 */
const defaultReplacer = event => {

    if (event.ctrlKey) {
        return undefined;
    }

    return (key, value) => {
        switch (key) {
            case 'createForm':
            case 'editForm':
                return undefined;
            default:
                return value;
        }
    };

};

/**
 *  On drag start, load the in-memory model ready to dropped (onto file system) or another element
 * @param {DragEvent} event
 * @param {LinkedRepresentation|Object} model
 * @param {?string|string[]} mediaType types to be attached to the event on drag
 * @return {boolean}
 */
export function dragstart(event, model, mediaType) {

    log.debug('[Drag] start');

    /**
     * Drop effect from drag can be set in dragenter and/or dragover
     *
     * You can modify the dropEffect property during the dragenter or dragover events, if
     * for example, a particular drop target only supports certain operations. You can modify
     * the dropEffect property to override the user effect, and enforce a specific drop operation
     * to occur. Note that this effect must be one listed within the effectAllowed property. Otherwise,
     * it will be set to an alternate value that is allowed.
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#drageffects
     */
    event.dataTransfer.effectAllowed = 'copy';
    event.dataTransfer.dropEffect = 'copy';

    event.srcElement.classList.add('drag');

    setDragData(model, event, defaultReplacer(event), mediaType);

    if (event.dataTransfer.types.length === 0) {
        log.warn('[Drag] drag data was not set');
    } else {
        log.debug(`[Drag] data drag set for [${[event.dataTransfer.types].join(',')}]`);
    }


}

/**
 * On drag end, change the icon effect to remove the 'drag' icon
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragend(event) {

    if (event.dataTransfer.dropEffect === 'none') {
        log.debug('[Drag] cancelled');
    } else {
        log.debug(`[Drag] complete ${event.dataTransfer.dropEffect}`);

    }

    event.target.classList.remove('drag');
    event.target.classList.remove('over');
}

/**
 * On drag over, change the icon effect to 'copy' and the target icon to 'over'
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragover(event) {

    log.debug('[Drag] in - over');

    /**
     * Drop effect from drag can be set in dragenter and/or dragover
     *
     * You can modify the dropEffect property during the dragenter or dragover events, if
     * for example, a particular drop target only supports certain operations. You can modify
     * the dropEffect property to override the user effect, and enforce a specific drop operation
     * to occur. Note that this effect must be one listed within the effectAllowed property. Otherwise,
     * it will be set to an alternate value that is allowed.
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#drageffects
     */
    event.dataTransfer.dropEffect = 'copy';

    event.target.classList.add('over');

    /**
     * Allows us to drop and we could either return false or event.preventDefault()
     *
     * If you want to allow a drop, you must prevent the default handling by cancelling
     * the event. You can do this either by returning false from an attribute-defined
     * event listener, or by calling the event's preventDefault() method. The latter
     * may be more feasible in a function defined in a separate script.
     *
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#droptargets
     */
    event.preventDefault();
    // return false;
}

/**
 * On drag enter, change the icon effect to 'over' on the target
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragenter(event) {

    log.debug('[Drag] in - enter');

    /**
     * Drop effect from drag can be set in dragenter and/or dragover
     *
     * You can modify the dropEffect property during the dragenter or dragover events, if
     * for example, a particular drop target only supports certain operations. You can modify
     * the dropEffect property to override the user effect, and enforce a specific drop operation
     * to occur. Note that this effect must be one listed within the effectAllowed property. Otherwise,
     * it will be set to an alternate value that is allowed.
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#drageffects
     */
    event.dataTransfer.dropEffect = 'copy';

    event.target.classList.add('over');

    /**
     * Allows us to drop and we could either return false or event.preventDefault()
     *
     * If you want to allow a drop, you must prevent the default handling by cancelling
     * the event. You can do this either by returning false from an attribute-defined
     * event listener, or by calling the event's preventDefault() method. The latter
     * may be more feasible in a function defined in a separate script.
     *
     * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#droptargets
     */
    event.preventDefault();
    // return false;
}

/**
 * On drag leave, remove the 'over' icon from the target. No cancel events are required
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragleave(event) {
    log.debug('[Drag] in - leave');

    event.target.classList.remove('over');
}

/**
 * On dropping, remove the 'over' icon from the target (to make the procees look finished) and then
 * dump the model to the relevant location based on the HTML 5 {@link DataTransfer}
 *
 * This has the side effect of a callback so that the calling ocmponent can get access to the model
 * in the cases that a model has been dropped from the desktop onto the browser.
 *
 * @param {DragEvent} event
 * @param {function(LinkedRepresentation)}cb callback function process the model
 * @param {string?} mediaType='application/json' media type to be returned
 * @return {boolean}
 */
export function drop(event, cb, mediaType) {

    mediaType = mediaType || 'application/json';

    log.debug('[Drag] in - drop');

    if (event.dataTransfer.items.length !== 0) {

        // Stops some browsers from redirecting.
        event.stopPropagation();

        event.target.classList.remove('over');

        const result = getDragData(event.dataTransfer, mediaType);

        if (result) {
            cb(result);
            /**
             * Accept the drop event
             *
             * Call the preventDefault() method of the
             * event if you have accepted the drop so that the default browser
             * handling does not handle the dropped data as well.
             *
             * @see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#drop
             */
            event.preventDefault();
            return false;

        }
    } else {
        log.debug('[Drop] event has no drop data');
    }

}

/**
 * Make a uri-list formatted string without comments of the the collection items
 * @param {CollectionRepresentation|string[]} representation
 * @returns {string} uri-list formatted (without comments)
 */
export function makeUriList(representation) {

    const arr = representation.items
        ? representation.items.map(({id}) => id)
        : representation || [];

    return arr.join('\n');
}


/**
 *
 * @param {string} uriList
 * @returns {string[]}
 */
export function fromUriList(uriList) {
    const list = uriList.split('\n');
    return list.filter(uri => !uri.startsWith('#'));

}
