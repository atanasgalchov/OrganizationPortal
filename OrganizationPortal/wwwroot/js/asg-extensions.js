(function () {
	$.fn.serializeToObject = function () {
		var $form = $(this);
		
		if ($form.length > 1) {
			var result = [];
			for (var form of $form) {
				result.push($(form).serializeToObject());
			}		

			return result;
		}

		var result = {};
		var dataArray = $form.serializeArray();
		if (dataArray) {
			for (var dataItem of dataArray) {
				var isKeyTypeArray = dataArray.filter(function () { return this.name == dataItem.name }).length > 1
				if (!result.hasOwnProperty(dataItem.name)) {
					result[dataItem.name] = isKeyTypeArray ? [] : '';
				}
				if (isKeyTypeArray) {
					result[dataItem.name].push(dataItem.value)
				} else {
					result[dataItem.name] = dataItem.value;
				}				
			}
		}
		
		return result
	}
})();