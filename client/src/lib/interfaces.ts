/**
 * An event bus interface (stolen from {@link Vue}
 */
export interface EventBus {
    $on(event: string | string[], callback: Function): this;
    $once(event: string, callback: Function): this;
    $off(event?: string | string[], callback?: Function): this;
    $emit(event: string, ...args: any[]): this;
}
