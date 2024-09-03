$(document).ready(function () {
    /*alert('ok');*/
    $('#tblData1').DataTable({
        "ajax": { url: '/Admin/Product/getall' },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" }
        ]
    });
});