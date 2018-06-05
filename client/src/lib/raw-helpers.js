/* global window document Blob */

// https://stackoverflow.com/questions/13405129/javascript-create-and-save-file
const saveToFile = (data, filename, type) => {
    const file = new Blob([data], {type: type});
    if (window.navigator.msSaveOrOpenBlob) // IE10+
        window.navigator.msSaveOrOpenBlob(file, filename);
    else { // Others
        const a = document.createElement('a'),
            url = URL.createObjectURL(file);
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        setTimeout(function () {
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }, 0);
    }
};

/**
 * Take a string and put it in the clipboard.
 *
 * from: https://hackernoon.com/copying-text-to-clipboard-with-javascript-df4d4988697f
 *
 * @example
 *
 *  copyToClipboard("str");
 *  copyToClipboard(JSON.stringify(this.representation, null, 2));  <-- stringify an object to show as JSON
 *
 *
 * @param {string} str
 */
const copyToClipboard = str => {
    /**
     * Create a new textarea that has the value to be copied
     * @returns {HTMLTextAreaElement}
     */
    function prepare() {
        const el = document.createElement('textarea');
        el.value = str;
        document.body.appendChild(el);
        return el;
    }

    /**
     * @param {HTMLTextAreaElement} el
     */
    function cleanup(el) {
        document.body.removeChild(el);
    }

    const el = prepare();

    // select is global that points to the text to be captured
    el.select();

    // now put into the clipboard the selected text
    document.execCommand('copy');

    cleanup(el);

};

export { saveToFile, copyToClipboard };
