--利用面向对象
Object:subClass("BasePanel")

BasePanel.panelObj=nil

BasePanel.controls={}

BasePanel.isInitEvent=false

function BasePanel:Init(name)
    if self.panelObj==nil then
        --实现各自的实例化
        self.panelObj=ABMgr:LoadRes("ui",name,typeof(GameObject))
        self.panelObj.transform:SetParent(Canvas,false)
        --得到控件 用得到children的方法得到挂载某个脚本的全部组件
        local allUI=self.panelObj:GetComponentsInChildren(typeof(CS.UnityEngine.EventSystems.UIBehaviour))
        for i = 0, allUI.Length-1 do
            --为了避免添加到我们不需要的控件
            --这里先约定要用到的控件的名字前缀加上这些前缀
            --有这些前缀才进行添加
            local uiName=allUI[i].name
            if string.find(uiName,"txt")~=nil or
               string.find(uiName,"img")~=nil or
               string.find(uiName,"sV")~=nil or
               string.find(uiName,"tog")~=nil or
               string.find(uiName,"btn")~=nil then
                --为了能够在得的时候 能够确定空间的类型 所以需要存储类型
                --利用反射 type 得到控件的类名
                local typeName=allUI[i]:GetType().Name
                --因为直接通过挂载的ui控件来找的 所以一个组件上的所以脚本都会被找到
                --这时候就会出现重名的问题 所以这里就要把这些脚本全部加入到一个表中来解决
                --先判断控件总表里这个键的值有没有
                if self.controls[uiName]~=nil then
                    --如果有就把这个类型作为键 对象作为值存进这个对象的表中
                    self.controls[uiName][typeName]=allUI[i]
                else
                    --如果没有就直接加
                    --但是还有一个问题 一个组件上的脚本都存在这里面
                    --那怎么得到呢 就需要用到GetType方法
                    --得到类型过后直接以字典的形式存进名字对应的表里面
                    self.controls[allUI[i].name]={[typeName]=allUI[i]}
                    --
                end
                
            end
        end
    end
end

function BasePanel:GetControl(name,typeName)
    if self.controls[name]~=nil then
        local sameNameControl=self.controls[name]
        if sameNameControl[typeName]~=nil then
            return sameNameControl[typeName]
        end
    end
    return nil
end

function BasePanel:ShowMe(name)
    self:Init(name)
    self.panelObj:SetActive(true)

end

function BasePanel:HideMe()
    self.panelObj:SetActive(false)
end