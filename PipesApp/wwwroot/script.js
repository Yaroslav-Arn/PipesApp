let currentController = '';
let data;

// Функция для вычисления итоговых данных
function calculateSummaryData(data) {
    // Инициализируем переменные для хранения общего количества, количества годных и брака, а также общего веса
    let totalCount = 0;
    let goodCount = 0;
    let defectCount = 0;
    let totalWeight = 0;

    // Проходим по каждой трубе в данных
    data.forEach(pipe => {
        // Увеличиваем общее количество на 1
        totalCount++;
        // Если труба годная, увеличиваем количество годных на 1, иначе увеличиваем количество брака
        if (pipe.quality) {
            goodCount++;
        } else {
            defectCount++;
        }
        // Добавляем вес трубы к общему весу
        totalWeight += pipe.weight;
    });

    // Возвращаем объект с итоговыми данными
    return {
        totalCount,
        goodCount,
        defectCount,
        totalWeight
    };
}

// Функция фильтрации
function applyFilters() {
    const qualityFilter = document.getElementById('qualityFilter').value;
    const packageFilter = document.getElementById('packageFilter').value;

    // Применяем выбранные фильтры
    const filteredData = data.filter(pipe => {
        // Фильтр по качеству
        if (qualityFilter !== 'all') {
            const expectedQuality = qualityFilter === 'good' ? true : false;
            if (pipe.quality !== expectedQuality) {
                return false;
            }
        }

        // Фильтр по номеру пакета
        if (packageFilter !== '') {
            if (pipe.packageId === null && packageFilter !== 'null') {
                return false;
            }
            if (pipe.packageId !== null && pipe.packageId !== parseInt(packageFilter)) {
                return false;
            }
        }

        return true;
    });

    // Обновляем таблицу с отфильтрованными данными
    displayPipesData(filteredData);
}
// Функция для отправки запроса на сервер и отображения данных таблицы
function fetchData(controllerName) {
    currentController = controllerName;
    fetch(`/api/${controllerName}`) 
        .then(response => response.json())
        .then(responseData => {
            data = responseData; 
            // Очистка текущей таблицы
            const tableContainer = document.getElementById('tableContainer');
            tableContainer.innerHTML = '';

            // Создание HTML таблицы на основе полученных данных
            const table = createTable(data);
            tableContainer.appendChild(table);


            const filterContainer = document.getElementById('filterContainer');
            if (controllerName === 'Pipes') {
                filterContainer.style.display = 'block';
                applyFilters();
            } else {
                filterContainer.style.display = 'none';
            }

        })
        .catch(error => {
            console.error('Ошибка при получении данных:', error);
        });
}

// Функция для создания HTML таблицы
function createTable(data) {
    const table = document.createElement('table');
    const headers = Object.keys(data[0]);

    // Создание заголовков таблицы
    const headerRow = document.createElement('tr');
    headers.forEach(headerText => {
        const header = document.createElement('th');
        header.textContent = headerText;
        headerRow.appendChild(header);
    });
    table.appendChild(headerRow);

    // Создание строк таблицы
    data.forEach(rowData => {
        const row = document.createElement('tr');
        headers.forEach(headerText => {
            const cell = document.createElement('td');
            cell.textContent = rowData[headerText];
            row.appendChild(cell);
        });
        table.appendChild(row);
    });

    return table;
}
//Функция отображения таблицы труб
function displayPipesData(data) {
    // Очищаем содержимое текущей таблицы
    const tableContainer = document.getElementById('tableContainer');
    tableContainer.innerHTML = '';

    // Создаем таблицу
    const table = document.createElement('table');
    table.classList.add('pipe-table');

    // Создаем заголовки
    const headers = ['Номер трубы', 'Качество', 'Длинна', 'Толщина', 'Диаметр', 'Вес', 'Марка стали', 'Номер пакета'];
    const headerRow = document.createElement('tr');
    headers.forEach(headerText => {
        const header = document.createElement('th');
        header.textContent = headerText;
        headerRow.appendChild(header);
    });
    table.appendChild(headerRow);

    // Заполняем таблицу данными
    data.forEach(pipe => {
        const row = document.createElement('tr');
        const steelGradeCell = document.createElement('td');

        // Получаем данные о марке стали по SteelGradeId
        fetchRecordById('SteelGrades', pipe.steelGradeId)
            .then(steelGradeData => {
                if (steelGradeData) {
                    // Вставляем значение Grade в ячейку
                    steelGradeCell.textContent = steelGradeData.grade;
                } else {
                    // Если данные не найдены или произошла ошибка, выводим "Н/Д"
                    steelGradeCell.textContent = 'Н/Д';
                }
            })
            .catch(error => {
                console.error('Ошибка при получении данных о марке стали:', error);
                // Если произошла ошибка, выводим "Н/Д"
                steelGradeCell.textContent = 'Н/Д';
            });

        // Добавляем остальные ячейки
        row.innerHTML = `
            <td>${pipe.id}</td>
            <td style="color: ${pipe.quality ? 'green' : 'red'}">${pipe.quality ? 'годная' : 'брак'}</td>
            <td>${pipe.length}</td>
            <td>${pipe.thickness}</td>
            <td>${pipe.diameter}</td>
            <td>${pipe.weight}</td>
        `;

        // Добавляем ячейку с маркой стали
        row.appendChild(steelGradeCell);

        // Добавляем ячейку с номером пакета
        const packageIdCell = document.createElement('td');
        packageIdCell.textContent = pipe.packageId || ''; // Если packageId null, то оставляем пустым
        row.appendChild(packageIdCell);

        // Добавляем строку в таблицу
        table.appendChild(row);
    });
    // Добавляем строку с итоговыми данными
    const summaryRow = document.createElement('tr');
    const summaryData = calculateSummaryData(data);
    summaryRow.innerHTML = `
        <td colspan="8">
            Общее количество: ${summaryData.totalCount}, 
            Годные: ${summaryData.goodCount}, 
            Брак: ${summaryData.defectCount}, 
            Общий вес: ${summaryData.totalWeight}
        </td>
    `;
    table.appendChild(summaryRow);

    // Добавляем таблицу на страницу
    tableContainer.appendChild(table);
}

// Функция для запроса данных записи по её ID
function fetchRecordById(controllerName, id) {
    return fetch(`/api/${controllerName}/${id}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Ошибка при получении данных');
            }
            return response.json();
        })
        .catch(error => {
            console.error(`Ошибка при получении данных по ID ${id} из ${controllerName}:`, error);
            return null;
        });
}
// Функция для открытия модального окна редактирования
function openEditModal() {
    fetch(`/api/${currentController}`)
        .then(response => response.json())
        .then(data => {
            const addForm = document.getElementById('editForm');
            addForm.innerHTML = '';
            
            if (currentController === 'Pipes') {
                openEditPipeModal(); // Вызываем функцию для добавления трубы
            } else {
                Object.keys(data[0]).forEach(key => {

                    const label = document.createElement('label');
                    label.textContent = key;
                    label.classList.add('input-label');
                    const input = document.createElement('input');
                    input.type = 'text';
                    input.name = key;
                    input.classList.add('input-field');
                    addForm.appendChild(label);
                    addForm.appendChild(input);
                });
                document.getElementById('editModal').style.display = 'block';
            }
        })
        .catch(error => {
            console.error('Ошибка при получении данных для формы добавления:', error);
        });
}

// Функция для отображения модального окна добавления данных
function openAddModal() {
    fetch(`/api/${currentController}`)
        .then(response => response.json())
        .then(data => {
            if (currentController === 'Pipes') {
                openAddPipeModal(); // Вызываем функцию для добавления трубы
            } else {
                const addForm = document.getElementById('addForm');
                addForm.innerHTML = '';
                let isFirstField = true; // Флаг для определения первого поля

                Object.keys(data[0]).forEach(key => {
                    // Пропускаем добавление первого поля
                    if (isFirstField) {
                        isFirstField = false;
                        return;
                    }

                    const label = document.createElement('label');
                    label.textContent = key;
                    label.classList.add('input-label');
                    const input = document.createElement('input');
                    input.type = 'text';
                    input.name = key;
                    input.classList.add('input-field');
                    addForm.appendChild(label);
                    addForm.appendChild(input);
                });
                document.getElementById('addModal').style.display = 'block';
            }
        })
        .catch(error => {
            console.error('Ошибка при получении данных для формы добавления:', error);
            showErrorModal(error.message);
        });
}
// Функция обновления таблицы труб
function openEditPipeModal() {
    fetch(`/api/SteelGrades`)
        .then(response => response.json())
        .then(steelGrades => {
            fetch(`/api/Packages`)
                .then(response => response.json())
                .then(packages => {
                    // Создаем форму для добавления трубы
                    const addForm = document.getElementById('editForm');
                    addForm.innerHTML = '';

                    // Добавляем поле Id
                    const idLabel = document.createElement('label');
                    idLabel.textContent = 'Id';
                    idLabel.classList.add('input-label');
                    const idInput = document.createElement('input');
                    idInput.type = 'number';
                    idInput.name = 'Id';
                    idInput.classList.add('input-field');
                    idInput.value = ''; // Пустое значение по умолчанию
                    addForm.appendChild(idLabel);
                    addForm.appendChild(idInput);

                    // Добавляем поле качества (годная/брак)
                    const qualityLabel = document.createElement('label');
                    qualityLabel.textContent = 'Качество';
                    qualityLabel.classList.add('input-label');
                    const qualitySelect = document.createElement('select');
                    qualitySelect.name = 'Quality';
                    qualitySelect.classList.add('input-field');
                    const qualityOptions = ['годная', 'брак'];
                    qualityOptions.forEach(optionText => {
                        const option = document.createElement('option');
                        option.value = optionText === 'годная';
                        option.textContent = optionText;
                        qualitySelect.appendChild(option);
                    });
                    addForm.appendChild(qualityLabel);
                    addForm.appendChild(qualitySelect);

                    // Добавляем поле марки стали (SteelGradeId)
                    const steelGradeLabel = document.createElement('label');
                    steelGradeLabel.textContent = 'Марка стали';
                    steelGradeLabel.classList.add('input-label');
                    const steelGradeSelect = document.createElement('select');
                    steelGradeSelect.name = 'SteelGradeId';
                    steelGradeSelect.classList.add('input-field');
                    steelGrades.forEach(steelGrade => {
                        const option = document.createElement('option');
                        option.value = steelGrade.id;
                        option.textContent = `${steelGrade.id}: ${steelGrade.grade}`;
                        steelGradeSelect.appendChild(option);
                    });
                    addForm.appendChild(steelGradeLabel);
                    addForm.appendChild(steelGradeSelect);

                    // Добавляем поле пакета (PackageId)
                    const packageLabel = document.createElement('label');
                    packageLabel.textContent = 'Номер пакета';
                    packageLabel.classList.add('input-label');
                    const packageSelect = document.createElement('select');
                    packageSelect.name = 'PackageId';
                    packageSelect.classList.add('input-field');
                    // Добавляем вариант "Нет пакета"
                    const noneOption = document.createElement('option');
                    noneOption.value = '0';
                    noneOption.textContent = 'Нет пакета';
                    packageSelect.appendChild(noneOption);
                    // Добавляем остальные пакеты
                    packages.forEach(package => {
                        const option = document.createElement('option');
                        option.value = package.id;
                        option.textContent = package.id.toString();
                        packageSelect.appendChild(option);
                    });
                    addForm.appendChild(packageLabel);
                    addForm.appendChild(packageSelect);

                    // Добавляем остальные поля
                    const otherFields = ['Length', 'Thickness', 'Diameter', 'Weight'];
                    otherFields.forEach(fieldName => {
                        const fieldLabel = document.createElement('label');
                        fieldLabel.textContent = fieldName;
                        fieldLabel.classList.add('input-label');
                        const fieldInput = document.createElement('input');
                        fieldInput.type = 'number';
                        fieldInput.name = fieldName;
                        fieldInput.classList.add('input-field');
                        fieldInput.value = ''; // Пустое значение по умолчанию
                        addForm.appendChild(fieldLabel);
                        addForm.appendChild(fieldInput);
                    });

                    // Открываем модальное окно для добавления трубы
                    document.getElementById('editModal').style.display = 'block';
                })
                .catch(error => {
                    console.error('Ошибка при получении данных о пакетах:', error);
                    showErrorModal('Ошибка при загрузке данных о пакетах');
                });
        })
        .catch(error => {
            console.error('Ошибка при получении данных о марках стали:', error);
            showErrorModal('Ошибка при загрузке данных о марках стали');
        });
}

// Функция для открытия модального окна добавления трубы
function openAddPipeModal() {
    fetch(`/api/SteelGrades`)
        .then(response => response.json())
        .then(steelGrades => {
            fetch(`/api/Packages`)
                .then(response => response.json())
                .then(packages => {
                    // Создаем форму для добавления трубы
                    const addForm = document.getElementById('addForm');
                    addForm.innerHTML = '';

                    // Добавляем поле качества (годная/брак)
                    const qualityLabel = document.createElement('label');
                    qualityLabel.textContent = 'Качество';
                    qualityLabel.classList.add('input-label');
                    const qualitySelect = document.createElement('select');
                    qualitySelect.name = 'Quality';
                    qualitySelect.classList.add('input-field');
                    const qualityOptions = ['годная', 'брак'];
                    qualityOptions.forEach(optionText => {
                        const option = document.createElement('option');
                        option.value = optionText === 'годная';
                        option.textContent = optionText;
                        qualitySelect.appendChild(option);
                    });
                    addForm.appendChild(qualityLabel);
                    addForm.appendChild(qualitySelect);

                    // Добавляем поле марки стали (SteelGradeId)
                    const steelGradeLabel = document.createElement('label');
                    steelGradeLabel.textContent = 'Марка стали';
                    steelGradeLabel.classList.add('input-label');
                    const steelGradeSelect = document.createElement('select');
                    steelGradeSelect.name = 'SteelGradeId';
                    steelGradeSelect.classList.add('input-field');
                    steelGrades.forEach(steelGrade => {
                        const option = document.createElement('option');
                        option.value = steelGrade.id;
                        option.textContent = `${steelGrade.id}: ${steelGrade.grade}`;
                        steelGradeSelect.appendChild(option);
                    });
                    addForm.appendChild(steelGradeLabel);
                    addForm.appendChild(steelGradeSelect);

                    // Добавляем поле пакета (PackageId)
                    const packageLabel = document.createElement('label');
                    packageLabel.textContent = 'Номер пакета';
                    packageLabel.classList.add('input-label');
                    const packageSelect = document.createElement('select');
                    packageSelect.name = 'PackageId';
                    packageSelect.classList.add('input-field');
                    // Добавляем вариант "Нет пакета"
                    const noneOption = document.createElement('option');
                    noneOption.value = '0';
                    noneOption.textContent = 'Нет пакета';
                    packageSelect.appendChild(noneOption);
                    // Добавляем остальные пакеты
                    packages.forEach(package => {
                        const option = document.createElement('option');
                        option.value = package.id;
                        option.textContent = package.id.toString();
                        packageSelect.appendChild(option);
                    });
                    addForm.appendChild(packageLabel);
                    addForm.appendChild(packageSelect);

                    // Добавляем остальные поля
                    const otherFields = ['Length', 'Thickness', 'Diameter', 'Weight'];
                    otherFields.forEach(fieldName => {
                        const fieldLabel = document.createElement('label');
                        fieldLabel.textContent = fieldName;
                        fieldLabel.classList.add('input-label');
                        const fieldInput = document.createElement('input');
                        fieldInput.type = 'number';
                        fieldInput.name = fieldName;
                        fieldInput.classList.add('input-field');
                        fieldInput.value = ''; // Пустое значение по умолчанию
                        addForm.appendChild(fieldLabel);
                        addForm.appendChild(fieldInput);
                    });

                    // Открываем модальное окно для добавления трубы
                    document.getElementById('addModal').style.display = 'block';
                })
                .catch(error => {
                    console.error('Ошибка при получении данных о пакетах:', error);
                    showErrorModal('Ошибка при загрузке данных о пакетах');
                });
        })
        .catch(error => {
            console.error('Ошибка при получении данных о марках стали:', error);
            showErrorModal('Ошибка при загрузке данных о марках стали');
        });
}

// Функция открытия модального окна для удаления
function openDeleteModal() {
    // Загружаем список ID записей в выпадающий список
    fetch(`/api/${currentController}`)
        .then(response => response.json())
        .then(data => {
            const deleteDataIdSelect = document.getElementById('deleteDataId');
            deleteDataIdSelect.innerHTML = ''; // Очищаем список перед заполнением

            // Создаем опции для каждого ID записи
            data.forEach(record => {
                const option = document.createElement('option');
                option.value = record.id;
                option.textContent = record.id;
                deleteDataIdSelect.appendChild(option);
            });

            // Открываем модальное окно для удаления записи
            document.getElementById('deleteModal').style.display = 'block';
        })
        .catch(error => {
            console.error('Ошибка при получении данных для удаления:', error);
            showErrorModal('Ошибка при загрузке данных для удаления');
        });
}

// Функция для закрытия модального окна для редактирования
function closeEditModal() {
    document.getElementById('editModal').style.display = 'none';
}

// Функция для закрытия модального окна для добавления
function closeAddModal() {
    document.getElementById('addModal').style.display = 'none';
}

// Функция для закрытия модального окна удаления
function closeDeleteModal() {
    document.getElementById('deleteModal').style.display = 'none';
}
// Функция для запроса PUT
function editData() {
    const formData = new FormData(document.getElementById('editForm'));
    const editedData = {};
    if (currentController === 'Pipes') {
        formData.forEach((value, key) => {
            // Преобразуем текстовое представление качества в булево значение
            if (key === 'Id') {
                editedData[key] = parseInt(value);
            }else
            if (key === 'Quality') {
                editedData[key] = value === 'true'; // true если годное, false если брак
            } else {
                editedData[key] = parseFloat(value); // Преобразуем в число
            }
        });

        console.log(editedData); // Отладочная информацияf
    }
    else {
        formData.forEach((value, key) => {
            editedData[key] = value;
        });
    }
    fetch(`/api/${currentController}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(editedData),
    })
        .then(response => {
            if (!response.ok) {
                // Если ответ с кодом 400, обрабатываем ошибку в формате JSON
                return response.json().then(error => {
                    throw new Error(error.title || 'Ошибка при изменении данных');
                });
            }
            return response.json();
        })
        .then(() => {
            fetchData(currentController); // После успешного обновления данных обновляем таблицу
            closeEditModal(); // Закрываем модальное окно
        })
        .catch(error => {
            console.error('Ошибка при изменении данных:', error);
            // Отображаем сообщение об ошибке в модальном окне
            showErrorModal(error.message);
        });
}


// Функция для запроса POST
function addData() {
    const formData = new FormData(document.getElementById('addForm'));
    const newData = {};
    if (currentController === 'Pipes') {
        formData.forEach((value, key) => {
            // Преобразуем текстовое представление качества в булево значение
            if (key === 'Quality') {
                newData[key] = value === 'true'; // true если годное, false если брак
            } else {
                newData[key] = parseFloat(value); // Преобразуем в число
            }
        });

        console.log(newData); // Отладочная информацияf
    }
    else {
        formData.forEach((value, key) => {
            newData[key] = value;
        });
    }
    fetch(`/api/${currentController}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(newData),
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Ошибка при добавлении данных');
            }
            return response.json();
        })
        .then(() => {
            fetchData(currentController);
            closeAddModal();
        })
        .catch(error => {
            console.error('Ошибка при добавлении данных:', error);
            // Отображаем сообщение об ошибке в модальном окне
            showErrorModal(error.message);
        });
}

// Функция для удаления записи
function deleteData() {
    const recordId = document.getElementById('deleteDataId').value;
    
    fetch(`/api/${currentController}/${recordId}`, {
        method: 'DELETE',
    })
        .then(response => {
            if (response.ok) {
                // Обновляем отображение данных после успешного удаления
                fetchData(currentController);
                closeDeleteModal(); // Закрываем модальное окно после успешного удаления
            } else {
                throw new Error('Ошибка при удалении записи');
            }
        })
        .catch(error => {
            console.error('Ошибка при удалении записи:', error);
            // Отображаем сообщение об ошибке
            showErrorModal('Ошибка при удалении записи');
        });
}


// Функция для отображения модального окна с сообщением об ошибке
function showErrorModal(message) {
    const errorModal = document.getElementById('errorModal');
    const errorMessageElement = document.getElementById('errorMessage');
    errorMessageElement.textContent = message;
    errorModal.style.display = 'block';
}

// Функция для закрытия модального окна с сообщением об ошибке
function closeErrorModal() {
    const errorModal = document.getElementById('errorModal');
    errorModal.style.display = 'none';
}


window.onload = () => fetchData('Pipes');