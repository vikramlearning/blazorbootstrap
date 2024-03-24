
export function initialize(dotNetHelper, elementId) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;

    if (Sortable) {
        const sortable = Sortable.create(listGroupEl);
    }
}
