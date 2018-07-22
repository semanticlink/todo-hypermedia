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
 * @param {DataTransfer} transfer - drag event
 * @param {string} mediaType
 * @returns {Promise.<LinkedRepresentation|Object>}
 */
const createModel = (transfer, mediaType) => {

    return new Promise((resolve, reject) => {

        log.debug(`[Drop] media type: ['${mediaType}']`);

        Object.entries(transfer.items).forEach(
            ([key, item]) => log.debug(`[Drag] item: kind '${item.kind}' type '${item.type}'`)
        );

        if (mediaType === 'application/json' && [...transfer.types].includes('application/json')) {

            log.debug('[Drop] found: application/json');

            return resolve(transfer.getData('application/json'));

        } else if (mediaType === 'text/uri-list' && _(transfer.items).any(item => item.kind === 'string' && item.type === 'text/uri-list')) {

            log.debug('[Drop] found: text/uri-list');

            let uriItem = _(transfer.items).find(item => item.kind === 'string' && item.type === 'text/uri-list');


            uriItem.getAsString(str => {
                log.debug(`[Drop] result: ${str}`);
                return resolve(str);
            });

        } else if (_(transfer.files).size() > 0) {

            // currently we are only going to take one file
            // TODO: multiple dropped files
            const file = _(transfer.files).first();
            log.debug('File: "' + file.name + '" of type ' + file.type);

            const extension = getFileExtension(file);
            // TODO: wider set of mime types
            if ('json' === extension) {

                const reader = new FileReader();
                reader.onload = function (evt) {
                    log.debug('[Drop] data loaded');
                    try {
                        return resolve(JSON.parse(evt.target.result));
                    } catch (err) {
                        log.error(err);
                        return reject(err);
                    }

                };
                reader.readAsText(file);
            } else {
                log.error();
                return reject('Unknown file type');
            }
        } else if (mediaType === 'text/plain' && _(transfer.types).contains('text/plain')) {
            //
            //  After all the content types and file have been processed, have a few guesses
            //  at what the content is. If it has come from an editor then JSON could be
            //  represented as text. Try parsing it as JSON and proceed if that works.
            //
            log.debug('[Drop] trying text as JSON');
            try {
                return resolve(JSON.parse(transfer.getData('Text')));
            } catch (e) {
                log.error('[Drop] error parsing text as JSON');
                if (e instanceof SyntaxError) {
                    return reject('The content is not valid JSON');
                }
                else {
                    return reject('The content type is not JSON and unsupported');
                }
            }
        } else {
            log.error(`[Drop] content is an unsupported file/content type: '${mediaType}'`);
            return reject('Unsupported drop type(s)');
        }

    });

};

/**
 * Name of a file dragged off the browser if we can't find a title.
 * @type {string}
 */
const DEFAULT_FILE_NAME = 'Unnamed';

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
 * @param {DataTransfer} dataTransfer
 * @param {function(key:string, value:string):string|undefined=} replacerStrategy JSON.stringify replacer function
 *         see https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify
 * @param {?string|string[]} mediaType media types to be included in the {@link DataTransfer} object on drag
 */
const dragLogic = (model, dataTransfer, replacerStrategy, mediaType) => {

    mediaType = mediaType || ['application/json', 'text/plain', 'DownloadUrl', 'text/uri-list'];
    mediaType = [mediaType];

    log.debug(`[Drop] using media types: [${mediaType.join(',')}]`);

    const canonicalJson = JSON.stringify(model, null, 1);

    // Alert: JSON.stringify can crash the browser on a large model with a complex replacer strategy
    //        If no strategy is handed in then we will use the canonical form
    let prettyJson = replacerStrategy ? JSON.stringify(model, replacerStrategy || null, 2) : canonicalJson;

    /**
     * the model should be {@link LinkedRepresentation} so there are search strategy. Title should actually
     * be retired.
     * TODO: we could use link relation 'self' to get titles, for example
     * @type {string}
     */
    const filename = (model.title || model.name || model.description || DEFAULT_FILE_NAME) + '.json';

    dataTransfer.effectAllowed = 'copy';
    dataTransfer.dropEffect = 'copy';

    if (mediaType.includes('application/json')) {
        dataTransfer.setData('application/json', canonicalJson);
        log.debug('[Drop] set data application/json');
    }

    if (mediaType.includes('DownloadUrl')) {
        // encode the prettyJson so that carriage returns are not lost
        // http://stackoverflow.com/questions/332872/encode-url-in-javascript
        // 'DownloadURL' is chrome specific (I think) see https://www.html5rocks.com/en/tutorials/casestudies/box_dnd_download/
        dataTransfer.setData(
            'DownloadURL',
            'application/json:' + filename + ':data:application/json,' + encodeURI(prettyJson)
        );
        log.debug('[Drop] set data DownloadURL');
    }

    if (mediaType.includes('text/plain')) {
        dataTransfer.setData('text/plain', prettyJson);
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
    event = event.originalEvent || event;

    // Check whether the element is draggable, since dragstart might be triggered on a child.
    if ((event.srcElement || event.originalTarget).draggable === 'false') {
        return true;
    }

    log.info('[Drag] start');

    dragLogic(model, event.dataTransfer, defaultReplacer(event), mediaType);

    (event.srcElement || event.originalTarget).classList.add('drag');

    event.stopPropagation();
}

/**
 * On drag end, change the icon effect to remove the 'drag' icon
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragend(event) {
    log.debug('[Drag] out - end ');
    event.target.classList.remove('drag');
    event.stopPropagation();
}

/**
 * On drag over, change the icon effect to 'copy' and the target icon to 'over'
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragover(event) {
    log.debug('[Drag] in - over');
    event = event.originalEvent || event;

    event.dataTransfer.dropEffect = 'copy';

    // allows us to drop
    if (event.preventDefault) {
        event.preventDefault();
    }
    event.target.classList.add('over');
    return false;
}

/**
 * On drag enter, change the icon effect to 'over' on the target
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragenter(event) {
    log.debug('[Drag] in - enter');
    event = event.originalEvent || event;

    event.target.classList.add('over');
    return false;
}

/**
 * On drag leave, remove the 'over' icon from the target
 * @param {DragEvent} event
 * @return {boolean}
 */
export function dragleave(event) {
    log.debug('[Drag] in - leave');
    event = event.originalEvent || event;

    event.target.classList.remove('over');
    return false;
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
    event = event.originalEvent || event;

    // Stops some browsers from redirecting.
    // see http://stackoverflow.com/questions/16701092/angularjs-multiple-ng-click-event-bubbling
    if (event.stopPropagation) {
        event.stopPropagation();
    }
    if (event.preventDefault) {
        event.preventDefault();
    }

    event.target.classList.remove('over');

    createModel(event.dataTransfer, mediaType)
        .then(model => cb(model));

    return false;
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
