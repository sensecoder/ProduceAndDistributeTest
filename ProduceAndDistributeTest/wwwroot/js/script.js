function formDataRequest() {
    let formInputs = document.forms["setupForm"].getElementsByClassName("input");
    let reqStr = "";
    for (input of formInputs) {
        reqStr += input.getAttribute("name") + "=" + input.value + "&";
    }
    reqStr += "__RequestVerificationToken=" + document.getElementsByName("__RequestVerificationToken").item(0).getAttribute("value");
    return reqStr;    
}

function onValidateSetup() {
    let action = document.forms["setupForm"].action;
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            switch (this.status) {
                case 200:
                    renderDocument(this.responseText);
                    renderAwait();
                    document.getElementById("btnStart").setAttribute("disabled", "true");
                    StartProcess();
                    break;
                default:
                    renderDocument(this.responseText);
            }           
        }
    }
    xhttp.open("POST", action);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send(formDataRequest());
    return false;
}

function StartProcess() {
    let action = "api/StartProcess";
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            switch (this.status) {
                case 200:
                    getStockLournal();
                    getDeliveryStatistic();
                    document.getElementById("btnStart").removeAttribute("disabled");
                    break;
                default:
                    alert("api/StartProcess status = " + this.status + "\r\n" +
                        "Что-то пошло не так... Попробуйте ещё раз!");
                    tableArea = document.getElementById("stockJournal");
                    tableArea.innerHTML = "";
                    statisticArea = document.getElementById("deliveryStatistic");
                    statisticArea.innerHTML = "";
                    document.getElementById("btnStart").removeAttribute("disabled");

            }
        }
    }
    xhttp.open("POST", action);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send(formDataRequest());
}

function getStockLournal(page = 0, itemsOnPage = 100) {
    let action = "api/StockJournal/?page=" + page + "&itemsOnPage=" + itemsOnPage;
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            switch (this.status) {
                case 200:
                    renderStockJournalPage(this.responseText, page, itemsOnPage);
                    break;
                default:
                    alert("api/StockJournal/?page=" + page + "&itemsOnPage=" + itemsOnPage + " status = " + this.status);
            }
        }
    }
    xhttp.open("GET", action);
    xhttp.send();
}

function getDeliveryStatistic() {
    let action = "api/DeliveryStatistic/";
    
    const xhttp = new XMLHttpRequest();
    
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            switch (this.status) {
                case 200:
                    renderDeliveryStatistic(this.responseText);
                    break;
                default:
                    alert("api/DeliveryStatistic/ status = " + this.status);
            }
        }
    }
    xhttp.open("GET", action);
    xhttp.send();
}

function renderDeliveryStatistic(listJSON) {
    list = JSON.parse(listJSON);
    statisticArea = document.getElementById("deliveryStatistic");
    statisticArea.innerHTML = "";
    statisticArea.setAttribute("text-align", "top");
    statisticArea.appendChild(centralTextH3("Статистика доставки"));
    table = document.createElement('table');
    table.setAttribute("class", "table");
    statisticArea.appendChild(table);
    tableHead = document.createElement('thead');
    productHead = document.createElement('tr');
    let cargoPlaces = 0;
    let productsNames = [];
    for (truck of list) { 
        let places = 0;
        for (cargo of truck.cargo) {
            productsNames[places] = cargo.productData.name;
            places++;
        }
        if (places > cargoPlaces) {
            cargoPlaces = places;
        }
    }
    for (name of productsNames) {
        let data = document.createElement('th');
        data.innerText = name;
        productHead.appendChild(data);
    }
    head = document.createElement('tr');
    append(head, th("Грузовик", { atrName: "rowspan", atrValue: "2" }, { atrName: "style", atrValue: "vertical-align: middle" }),
                 th("Вмес-ть", { atrName: "rowspan", atrValue: "2" }, { atrName: "style", atrValue: "vertical-align: middle" }),
                 th("Среднее перевозимое кол-во продукции", { atrName: "colspan", atrValue: cargoPlaces.toString() }),
                 th("Сум.ср", { atrName: "rowspan", atrValue: "2" }, { atrName: "style", atrValue: "vertical-align: middle; color: lightgrey" })
    );
    tableHead.appendChild(head);
    tableHead.appendChild(productHead);
    table.appendChild(tableHead);
    tableBody = document.createElement('tbody');
    for (truck of list) {
        row = document.createElement('tr');
        let data = document.createElement('td');
        data.innerText = truck.name;
        row.appendChild(data);
        data = document.createElement('td');
        data.innerText = truck.capacity;
        row.appendChild(data);
        let avgLoad = 0;
        for (name of productsNames) {
            let data = document.createElement('td');
            let quantity = 0;
            for (product of truck.cargo) {
                if (product.productData.name === name) {
                    quantity = +product.quantity;
                }
            }
            data.innerText = quantity;
            avgLoad += quantity;
            row.appendChild(data);
        }
        data = document.createElement('td');
        data.innerText = avgLoad;
        data.setAttribute("style", "color: lightgrey");
        row.appendChild(data);
        tableBody.appendChild(row);
    }
    table.appendChild(tableBody);
}

function renderStockJournalPage(listJSON, page, itemsOnPage) {
    list = JSON.parse(listJSON);
    if (list.length == 0) {
        document.getElementById("buttonNextPage").setAttribute("hidden", "true");
        alert("Больше нет записей!");
        return;
    }
    tableArea = document.getElementById("stockJournal");
    tableArea.innerHTML = "";
    tableArea.appendChild(centralTextH3("Журнал склада"));
    table = document.createElement('table');
    table.setAttribute("class", "table table-striped");
    tableArea.appendChild(table);
    tableHead = document.createElement('thead');
    head = document.createElement('tr');
    append(head, th("п/п"), th("Фабрика"), th("Продукт"), th("Кол-во"));
    tableHead.appendChild(head);
    table.appendChild(tableHead);
    tableBody = document.createElement('tbody');
    items = 0;
    for (record of list) {
        row = document.createElement('tr');
        append(row, td(record.pos),
                    td(record.productData.factory),
                    td(record.productData.name),
                    td(record.quantity));
        tableBody.appendChild(row);
        items++;
    }
    table.appendChild(tableBody);
    pageDiv = centralTextDiv("Показаны записи с " + ((itemsOnPage * page) + 1) +
        " по " + (items + (itemsOnPage * page)));
    tableArea.appendChild(pageDiv);
    if (items == itemsOnPage) {
        buttonNextPage = document.createElement('button');
        buttonNextPage.setAttribute("class", "btn btn-primary");
        buttonNextPage.setAttribute("id", "buttonNextPage");
        buttonNextPage.setAttribute("type", "button");
        buttonNextPage.setAttribute("onclick", "getStockLournal(page = " + (page + 1) + ")");
        buttonNextPage.innerText = "Следующая страница";
        tableArea.appendChild(buttonNextPage);
    }
}

function renderDocument(text) {
    document.open();
    document.write(text);
    document.close();
}

function renderAwait() {
    tableArea = document.getElementById("stockJournal");
    tableArea.innerHTML = "";
    statisticArea = document.getElementById("deliveryStatistic");
    statisticArea.innerHTML = "";
    awaitGif = document.createElement('img');
    awaitGif.setAttribute("src", "/pic/await.gif");
    awaitGif.setAttribute("width", "80");
    awaitGif.setAttribute("height", "80");
    tableArea.appendChild(awaitGif);
}

function centralTextH3(text) {
    result = document.createElement('h3');
    result.innerText = text;
    result.setAttribute("class", "text-center");
    return result;
}
function centralTextDiv(text) {
    result = document.createElement('div');
    result.innerText = text;
    result.setAttribute("class", "text-center");
    return result;
}

function append(element) {
    for (let i = 1; i < arguments.length; i++) {
        element.appendChild(arguments[i]);
    }
}

function td(str) {
    data = document.createElement('td');
    //data.setAttribute("style","text-align:center")
    data.innerText = str;
    return data;
}

function th(str) {
    data = document.createElement('th');
    for (arg of arguments) {
        data.setAttribute(arg.atrName, arg.atrValue);
    }
    data.innerText = str;
    return data;
}