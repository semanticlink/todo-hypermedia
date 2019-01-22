import {LinkedRepresentation, CollectionRepresentation} from "semantic-link";
import {SyncOptions, DifferencerOptions} from "./sync/interfaces";
import {
    LinkOptions,
    EditMergeOptions,
    StateOptions,
    CreateMergeOptions,
    SparseResourceOptions, CreateCollectionResourceItemOptions, UpdateCollectionResourceItemOptions
} from "./cache/interfaces";
import {QueryOptions} from "./query/interfaces";

export type UriList = string | string[];

export type Representation = (CollectionRepresentation | FormRepresentation | UriList | any) & LinkedRepresentation;

/**
 * Known set of field types from the semantic link. Maps the representation types to the known types that
 * can be rendered (input not select at this stage)
 *
 * @see https://bootstrap-vue.js.org/docs/components/form-input
 *
 *      Caveats with input types:
 *      - Not all browsers support all input types, nor do some types render in the same format across browser types/version.
 *      - Browsers that do not support a particular type will fall back to a text input type. As an example,
 *        Firefox desktop doesn't support date, datetime, or time, while Firefox mobile does.
 *      - Chrome lost support for datetime in version 26, Opera in version 15, and Safari in iOS 7. Instead
 *        of using datetime, since support should be deprecated, use date and time as two separate input types.
 *      - For date and time style input, where supported, the displayed value in the GUI may be different than what
 *        is returned by its value.
 *      - Regardless of input type, the value is always returned as a string representation.
 */
export enum FieldType {
    Text = 'http://types/text',
    Password = 'http://types/text/password',
    Email = 'http://types/text/email',
    Checkbox = 'http://types/text/check',
    Date = 'http://types/text/date',
    DateTime = 'http://types/text/datetime',
    Select = 'http://types/text/select',
}

/**
 * The current types of form inputs that are supported from semantic link
 *
 * @remarks
 *
 * Note: these are hard coded in {@link ResourceMerger} and have avoided enums because of the mix of typescript and javascript
 */
export type FormType =
    FieldType.Text |
    FieldType.Password |
    FieldType.Email |
    FieldType.Checkbox |
    FieldType.Date |
    FieldType.DateTime |
    FieldType.Select

export interface FormItem {
    readonly type: FormType | string;
    readonly name: string;
    readonly description?: string;
    readonly required?: boolean;
    readonly items?: FormItem[];
}

export interface FormRepresentation extends LinkedRepresentation {
    items: FormItem[]
}

export interface UtilOptions extends LinkOptions,
    StateOptions,
    SyncOptions,
    DifferencerOptions,
    EditMergeOptions,
    CreateMergeOptions,
    SparseResourceOptions,
    CreateCollectionResourceItemOptions,
    UpdateCollectionResourceItemOptions,
    QueryOptions {
}
